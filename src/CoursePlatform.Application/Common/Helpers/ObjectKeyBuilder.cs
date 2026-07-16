namespace CoursePlatform.Application.Common.Helpers;

public static class ObjectKeyBuilder
{
    public static string LessonVideoObjectKey(Guid courseId, Guid lessonId)
        => $"courses/{courseId:D}/lessons/{lessonId:D}/video/source";

    public static string CourseThumbnailObjectKey(Guid courseId)
        => $"courses/{courseId:D}/thumbnail/source";

    public static string CertificatePdfObjectKey(Guid certificateId)
        => $"certificates/{certificateId:D}/certificate.pdf";
}

