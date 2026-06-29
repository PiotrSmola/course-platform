using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Modules.Commands.DeleteModule;

public record DeleteModuleCommand(Guid CourseId, Guid ModuleId) : IRequest;

public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
    }
}

public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteModuleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await CourseAccessHelper.GetManagedModuleAsync(
            _context, _userManager, _currentUserService, request.CourseId, request.ModuleId, cancellationToken);

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
