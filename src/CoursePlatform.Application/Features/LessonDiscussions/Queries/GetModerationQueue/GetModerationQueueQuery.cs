using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonDiscussions.Queries.GetModerationQueue;

public record ModerationAnswerPreviewDto(
    Guid Id,
    string Body,
    string AuthorName,
    bool IsInstructorAnswer,
    DateTime CreatedAt);

public record ModerationQuestionDto(
    Guid Id,
    string Body,
    string AuthorName,
    string CourseTitle,
    string LessonTitle,
    Guid CourseId,
    Guid LessonId,
    DateTime CreatedAt,
    int AnswersCount,
    bool HasInstructorAnswer,
    IReadOnlyList<ModerationAnswerPreviewDto> AnswersPreview);

public record ModerationQueueDto(
    IReadOnlyList<ModerationQuestionDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public record GetModerationQueueQuery(
    Guid? CourseId = null,
    bool UnansweredByInstructorOnly = false,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<ModerationQueueDto>;

public class GetModerationQueueQueryHandler : IRequestHandler<GetModerationQueueQuery, ModerationQueueDto>
{
    private const int PreviewAnswerLimit = 3;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetModerationQueueQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ModerationQueueDto> Handle(GetModerationQueueQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        if (!_currentUserService.IsAdmin)
        {
            if (request.CourseId.HasValue)
            {
                await CourseAccessHelper.GetManagedCourseAsync(
                    _context, _currentUserService, request.CourseId.Value, cancellationToken);
            }
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 50 ? 20 : request.PageSize;

        var query = _context.LessonQuestions
            .AsNoTracking()
            .Where(q => !q.IsDeleted);

        if (!_currentUserService.IsAdmin)
        {
            var instructorId = _currentUserService.UserId.Value;
            query = query.Where(q => q.Lesson.Module.Course.InstructorId == instructorId);
        }

        if (request.CourseId.HasValue)
        {
            query = query.Where(q => q.Lesson.Module.CourseId == request.CourseId.Value);
        }

        if (request.UnansweredByInstructorOnly)
        {
            query = query.Where(q => !q.Answers.Any(a => !a.IsDeleted && a.IsInstructorAnswer));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new
            {
                q.Id,
                q.Body,
                AuthorName = q.Author.FirstName + " " + q.Author.LastName,
                CourseTitle = q.Lesson.Module.Course.Title,
                LessonTitle = q.Lesson.Title,
                CourseId = q.Lesson.Module.CourseId,
                q.LessonId,
                q.CreatedAt,
                AnswersCount = q.Answers.Count(a => !a.IsDeleted),
                HasInstructorAnswer = q.Answers.Any(a => !a.IsDeleted && a.IsInstructorAnswer),
                AnswersPreview = q.Answers
                    .Where(a => !a.IsDeleted)
                    .OrderBy(a => a.CreatedAt)
                    .Take(PreviewAnswerLimit)
                    .Select(a => new
                    {
                        a.Id,
                        a.Body,
                        AuthorName = a.Author.FirstName + " " + a.Author.LastName,
                        a.IsInstructorAnswer,
                        a.CreatedAt
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var items = questions.Select(q => new ModerationQuestionDto(
            q.Id,
            q.Body,
            q.AuthorName.Trim(),
            q.CourseTitle,
            q.LessonTitle,
            q.CourseId,
            q.LessonId,
            q.CreatedAt,
            q.AnswersCount,
            q.HasInstructorAnswer,
            q.AnswersPreview.Select(a => new ModerationAnswerPreviewDto(
                a.Id,
                a.Body,
                a.AuthorName.Trim(),
                a.IsInstructorAnswer,
                a.CreatedAt
            )).ToList()
        )).ToList();

        return new ModerationQueueDto(items, pageNumber, pageSize, totalCount, totalPages);
    }
}
