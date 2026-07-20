using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CoursePlatform.IntegrationTests;

public class CoursesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CoursesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCourses_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/courses?pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CoursesResponse>();
        body.Should().NotBeNull();
        body!.Items.Should().NotBeNull();
    }
}

public record CoursesResponse(List<CourseItem> Items, int TotalCount);
public record CourseItem(Guid Id, string Title);

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"CoursePlatformTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Testing:DatabaseName", _databaseName);
        builder.UseSetting("Jwt:Key", "integration-test-jwt-signing-key-32chars");
        builder.UseSetting("Jwt:Issuer", "CoursePlatform");
        builder.UseSetting("Jwt:Audience", "CoursePlatform");
        builder.UseSetting("Frontend:BaseUrl", "http://localhost:5173");
    }
}
