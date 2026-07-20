using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

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
    private readonly IAppCache _cache;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DeleteModuleCommandHandler> _logger;

    public DeleteModuleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage,
        ILogger<DeleteModuleCommandHandler> logger,
        IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cache = cache;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await CourseAccessHelper.GetManagedModuleAsync(
            _context, _currentUserService, request.CourseId, request.ModuleId, cancellationToken);

        var videoObjectKeys = await _context.Lessons
            .Where(l => l.ModuleId == request.ModuleId && !string.IsNullOrEmpty(l.VideoObjectKey))
            .Select(l => l.VideoObjectKey!)
            .ToListAsync(cancellationToken);

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);

        foreach (var objectKey in videoObjectKeys)
        {
            try
            {
                await _fileStorage.DeleteObjectAsync(objectKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete video object {ObjectKey} for removed module {ModuleId}.",
                    objectKey, request.ModuleId);
            }
        }
    }
}
