using System.Net;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Application.Features.Support.Commands.SubmitSupportMessage;

public record SubmitSupportMessageCommand(
    string Name,
    string Email,
    string Subject,
    string Message) : IRequest;

public class SubmitSupportMessageCommandValidator : AbstractValidator<SubmitSupportMessageCommand>
{
    public SubmitSupportMessageCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Subject)
            .NotEmpty()
            .MaximumLength(160)
            .Must(value => string.IsNullOrEmpty(value) || (!value.Contains('\r') && !value.Contains('\n')));
        RuleFor(command => command.Message).NotEmpty().MaximumLength(5000);
    }
}

public class SubmitSupportMessageCommandHandler : IRequestHandler<SubmitSupportMessageCommand>
{
    private readonly IEmailQueue _emailQueue;
    private readonly SupportOptions _supportOptions;

    public SubmitSupportMessageCommandHandler(
        IEmailQueue emailQueue,
        IOptions<SupportOptions> supportOptions)
    {
        _emailQueue = emailQueue;
        _supportOptions = supportOptions.Value;
    }

    public Task Handle(SubmitSupportMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_supportOptions.InboxAddress))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(string.Empty, "Kontakt jest chwilowo niedostępny.")
            });
        }

        var name = WebUtility.HtmlEncode(request.Name.Trim());
        var email = WebUtility.HtmlEncode(request.Email.Trim());
        var subject = request.Subject.Trim();
        var message = WebUtility.HtmlEncode(request.Message.Trim()).Replace("\n", "<br>", StringComparison.Ordinal);
        var html = $"""
            <!doctype html>
            <html lang="pl">
            <body>
              <h2>Nowa wiadomość kontaktowa</h2>
              <p><strong>Nadawca:</strong> {name} ({email})</p>
              <p><strong>Temat:</strong> {WebUtility.HtmlEncode(subject)}</p>
              <p>{message}</p>
            </body>
            </html>
            """;

        _emailQueue.Enqueue(new EmailMessage(
            _supportOptions.InboxAddress,
            $"[Kontakt] {subject}",
            html));

        return Task.CompletedTask;
    }
}
