namespace CoursePlatform.Application.Common.Authorization;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string InstructorOrAdmin = "InstructorOrAdmin";
    public const string ManageCourse = "ManageCourse";
}
