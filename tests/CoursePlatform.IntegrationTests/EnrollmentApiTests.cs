using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CoursePlatform.IntegrationTests;

public class EnrollmentApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EnrollmentApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Enroll_FreeCourse_ThenMyEnrollmentsContainProgress()
    {
        var (courseId, _, _, studentEmail) = await IntegrationTestHelpers.SeedPublishedCourseWithLessonAsync(_factory);
        var auth = await IntegrationTestHelpers.LoginAsync(_client, studentEmail);
        IntegrationTestHelpers.Authenticate(_client, auth.Token);

        var enrollResponse = await _client.PostAsJsonAsync("/api/enrollments", new { CourseId = courseId });
        enrollResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var myResponse = await _client.GetAsync("/api/enrollments/my");
        myResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var enrollments = await myResponse.Content.ReadFromJsonAsync<List<EnrollmentDto>>();
        enrollments.Should().NotBeNull();
        var item = enrollments!.Should().ContainSingle(e => e.CourseId == courseId).Subject;
        item.TotalLessons.Should().Be(1);
        item.CompletedLessons.Should().Be(0);
        item.ProgressPercentage.Should().Be(0);
        item.ContinueLessonId.Should().NotBeNull();
        item.FirstLessonId.Should().NotBeNull();
    }

    private sealed record EnrollmentDto(
        Guid Id,
        Guid CourseId,
        string CourseTitle,
        string? CourseThumbnailUrl,
        int CourseLevel,
        DateTime EnrolledAt,
        int CompletedLessons,
        int TotalLessons,
        double ProgressPercentage,
        Guid? FirstLessonId,
        Guid? ContinueLessonId);
}
