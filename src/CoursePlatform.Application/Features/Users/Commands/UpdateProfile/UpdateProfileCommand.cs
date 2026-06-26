using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Validation;
using FluentValidation;

namespace CoursePlatform.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(string FirstName, string LastName) : IRequest<UserProfileSummaryDto>;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserProfileSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileSummaryDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
            throw new ForbiddenAccessException();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", _currentUserService.UserId.Value.ToString());

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await _context.SaveChangesAsync(cancellationToken);

        return new UserProfileSummaryDto(user.Id, user.Email, user.FirstName, user.LastName);
    }
}

public record UserProfileSummaryDto(Guid Id, string? Email, string FirstName, string LastName);

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).ValidName("Imię");
        RuleFor(x => x.LastName).ValidName("Nazwisko");
    }
}
