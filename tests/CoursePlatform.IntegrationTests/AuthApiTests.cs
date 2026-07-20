using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CoursePlatform.IntegrationTests;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithUniqueEmail_ReturnsOkWithTokens()
    {
        var email = $"student-{Guid.NewGuid():N}@example.com";
        var payload = new
        {
            Email = email,
            Password = "Student123!",
            FirstName = "Jan",
            LastName = "Kowalski"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be(email);
        body.Token.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.Roles.Should().Contain("Student");
    }

    [Fact]
    public async Task Login_WithRegisteredUser_ReturnsOk()
    {
        var email = $"student-{Guid.NewGuid():N}@example.com";
        const string password = "Student123!";
        var registerPayload = new
        {
            Email = email,
            Password = password,
            FirstName = "Anna",
            LastName = "Nowak"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginPayload = new { Email = email, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be(email);
        body.Token.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_ReturnsNoContent()
    {
        var payload = new { Email = $"unknown-{Guid.NewGuid():N}@example.com" };

        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetCourses_WithoutAuth_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/courses?pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public record AuthResponse(
    Guid Id,
    string? Email,
    string FirstName,
    string LastName,
    string Token,
    string RefreshToken,
    List<string> Roles);
