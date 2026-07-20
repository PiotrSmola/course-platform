using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CoursePlatform.IntegrationTests;

public class LessonAccessApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LessonAccessApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLesson_WithoutEnrollment_ReturnsForbidden()
    {
        var (courseId, lessonId, _, studentEmail) = await IntegrationTestHelpers.SeedPublishedCourseWithLessonAsync(_factory);
        var auth = await IntegrationTestHelpers.LoginAsync(_client, studentEmail);
        IntegrationTestHelpers.Authenticate(_client, auth.Token);

        var response = await _client.GetAsync($"/api/courses/{courseId}/lessons/{lessonId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLessonVideo_WithoutEnrollment_ReturnsForbidden()
    {
        var (courseId, lessonId, _, studentEmail) = await IntegrationTestHelpers.SeedPublishedCourseWithLessonAsync(_factory);
        var auth = await IntegrationTestHelpers.LoginAsync(_client, studentEmail);
        IntegrationTestHelpers.Authenticate(_client, auth.Token);

        var response = await _client.GetAsync($"/api/courses/{courseId}/lessons/{lessonId}/video");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLesson_WithEnrollment_ReturnsOk()
    {
        var (courseId, lessonId, _, studentEmail) = await IntegrationTestHelpers.SeedPublishedCourseWithLessonAsync(_factory);
        var auth = await IntegrationTestHelpers.LoginAsync(_client, studentEmail);
        IntegrationTestHelpers.Authenticate(_client, auth.Token);

        var enrollResponse = await _client.PostAsJsonAsync("/api/enrollments", new { CourseId = courseId });
        enrollResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.GetAsync($"/api/courses/{courseId}/lessons/{lessonId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LessonDto>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(lessonId);
        body.Title.Should().Be("Lesson 1");
    }

    [Fact]
    public async Task GetLesson_Unauthenticated_ReturnsUnauthorized()
    {
        var (courseId, lessonId, _, _) = await IntegrationTestHelpers.SeedPublishedCourseWithLessonAsync(_factory);
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/api/courses/{courseId}/lessons/{lessonId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record LessonDto(Guid Id, string Title);
}
