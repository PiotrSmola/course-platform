using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonDiscussions.Queries.GetLessonDiscussion;

public record LessonAnswerDto(
    Guid Id,
    string Body,
    Guid AuthorId,
    string AuthorName,
    bool IsInstructorAnswer,
    DateTime CreatedAt,
    bool CanDelete);

public record LessonQuestionDto(
    Guid Id,
    string Body,
    Guid AuthorId,
    string AuthorName,
    DateTime CreatedAt,
    bool CanDelete,
    IReadOnlyList<LessonAnswerDto> Answers);

public record LessonDiscussionDto(
    IReadOnlyList<LessonQuestionDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public record GetLessonDiscussionQuery(
    Guid CourseId,
    Guid LessonId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<LessonDiscussionDto>;

public class GetLessonDiscussionQueryHandler : IRequestHandler<GetLessonDiscussionQuery, LessonDiscussionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonDiscussionQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LessonDiscussionDto> Handle(GetLessonDiscussionQuery request, CancellationToken cancellationToken)
    {
        var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You must be enrolled in the course to view the discussion.");
        }

        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (!lessonExists)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 50 ? 20 : request.PageSize;

        var query = _context.LessonQuestions
            .AsNoTracking()
            .Where(q => q.LessonId == request.LessonId && !q.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var userId = _currentUserService.UserId;
        var canModerate = false;

        if (userId != null)
        {
            canModerate = _currentUserService.IsAdmin
                || await _context.Courses.AnyAsync(
                    c => c.Id == request.CourseId && c.InstructorId == userId.Value,
                    cancellationToken);
        }

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new
            {
                q.Id,
                q.Body,
                q.AuthorId,
                AuthorName = q.Author.FirstName + " " + q.Author.LastName,
                q.CreatedAt,
                Answers = q.Answers
                    .Where(a => !a.IsDeleted)
                    .OrderBy(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.Body,
                        a.AuthorId,
                        AuthorName = a.Author.FirstName + " " + a.Author.LastName,
                        a.IsInstructorAnswer,
                        a.CreatedAt
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var items = questions.Select(q => new LessonQuestionDto(
            q.Id,
            q.Body,
            q.AuthorId,
            q.AuthorName.Trim(),
            q.CreatedAt,
            canModerate || (userId != null && q.AuthorId == userId.Value),
            q.Answers.Select(a => new LessonAnswerDto(
                a.Id,
                a.Body,
                a.AuthorId,
                a.AuthorName.Trim(),
                a.IsInstructorAnswer,
                a.CreatedAt,
                canModerate || (userId != null && a.AuthorId == userId.Value)
            )).ToList()
        )).ToList();

        return new LessonDiscussionDto(items, pageNumber, pageSize, totalCount, totalPages);
    }
}
