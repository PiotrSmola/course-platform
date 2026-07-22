using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class LessonResource : BaseEntity
{
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int Order { get; set; }

    public static LessonResource Create(
        Guid id,
        Guid lessonId,
        string title,
        string objectKey,
        string contentType,
        long sizeBytes,
        int order)
    {
        return new LessonResource
        {
            Id = id,
            LessonId = lessonId,
            Title = title,
            ObjectKey = objectKey,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Order = order
        };
    }
}
