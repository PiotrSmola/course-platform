namespace CoursePlatform.Application.Features.Quizzes;

public record QuizOptionInputDto(string Text, bool IsCorrect, int Order);

public record QuizQuestionInputDto(string Prompt, int Order, IReadOnlyList<QuizOptionInputDto> Options);

public record UpsertQuizRequest(
    string Title,
    int PassThresholdPercent,
    IReadOnlyList<QuizQuestionInputDto> Questions);

public record QuizOptionStudentDto(Guid Id, string Text, int Order);

public record QuizQuestionStudentDto(Guid Id, string Prompt, int Order, IReadOnlyList<QuizOptionStudentDto> Options);

public record LessonQuizStudentDto(
    Guid Id,
    string Title,
    int PassThresholdPercent,
    IReadOnlyList<QuizQuestionStudentDto> Questions);

public record QuizOptionInstructorDto(Guid Id, string Text, bool IsCorrect, int Order);

public record QuizQuestionInstructorDto(
    Guid Id,
    string Prompt,
    int Order,
    IReadOnlyList<QuizOptionInstructorDto> Options);

public record LessonQuizInstructorDto(
    Guid Id,
    string Title,
    int PassThresholdPercent,
    IReadOnlyList<QuizQuestionInstructorDto> Questions);

public record QuizAttemptAnswerInputDto(Guid QuestionId, Guid SelectedOptionId);

public record QuizAttemptResultDto(
    Guid AttemptId,
    int ScorePercent,
    bool Passed,
    DateTime SubmittedAt);

public record QuizAttemptListItemDto(
    Guid Id,
    int ScorePercent,
    bool Passed,
    DateTime SubmittedAt);
