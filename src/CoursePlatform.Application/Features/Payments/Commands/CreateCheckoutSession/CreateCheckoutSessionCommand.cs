using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Payments.Commands.CreateCheckoutSession;

public record CheckoutSessionDto(string? RedirectUrl, bool Enrolled);

public record CreateCheckoutSessionCommand(Guid CourseId) : IRequest<CheckoutSessionDto>;

public class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}

public class CreateCheckoutSessionCommandHandler : IRequestHandler<CreateCheckoutSessionCommand, CheckoutSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentGateway _paymentGateway;

    public CreateCheckoutSessionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentGateway paymentGateway)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentGateway = paymentGateway;
    }

    public async Task<CheckoutSessionDto> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        if (course.Status != CourseStatus.Published)
        {
            throw new ForbiddenAccessException("Course is not published.");
        }

        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId, cancellationToken);

        if (alreadyEnrolled)
        {
            throw new ValidationException(new[] { new ValidationFailure("CourseId", "Jesteś już zapisany na ten kurs.") });
        }

        if (course.Price <= 0)
        {
            await EnrollDirectlyAsync(userId, request.CourseId, cancellationToken);
            return new CheckoutSessionDto(null, true);
        }

        if (!_paymentGateway.IsConfigured)
        {
            throw new ValidationException(new[] { new ValidationFailure("CourseId", "Płatności są chwilowo niedostępne.") });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new ForbiddenAccessException("User not found.");
        }

        var payment = new Payment
        {
            UserId = userId,
            CourseId = course.Id,
            Amount = course.Price,
            Status = PaymentStatus.Pending
        };

        var session = await _paymentGateway.CreateCheckoutSessionAsync(
            payment.Id,
            course.Id,
            course.Title,
            course.Price,
            user.Email ?? string.Empty,
            cancellationToken);

        payment.StripeSessionId = session.SessionId;
        payment.Currency = session.Currency;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return new CheckoutSessionDto(session.RedirectUrl, false);
    }

    private async Task EnrollDirectlyAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        _context.Enrollments.Add(new Enrollment
        {
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, cancellationToken);

            if (!alreadyEnrolled)
            {
                throw;
            }
        }
    }
}
