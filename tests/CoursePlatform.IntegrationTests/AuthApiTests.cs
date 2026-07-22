using System.Net;
using System.Net.Http.Json;
using CoursePlatform.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CoursePlatform.IntegrationTests;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithUniqueEmail_ReturnsOkWithoutTokens()
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
        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be(email);
        body.Message.Should().NotBeNullOrWhiteSpace();
        body.FirstName.Should().Be("Jan");
    }

    [Fact]
    public async Task Login_BeforeEmailConfirmation_ReturnsBadRequest()
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

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Email");
        problem.Errors["Email"].Should().Contain(m => m.Contains("email", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Login_AfterEmailConfirmation_ReturnsOk()
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

        await ConfirmEmailAsync(email);

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
    public async Task ConfirmEmail_WithValidToken_ReturnsNoContent()
    {
        var email = $"student-{Guid.NewGuid():N}@example.com";
        var registerPayload = new
        {
            Email = email,
            Password = "Student123!",
            FirstName = "Ewa",
            LastName = "Test"
        };

        (await _client.PostAsJsonAsync("/api/auth/register", registerPayload))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        string token;
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
        }

        var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", new { Email = email, Token = token });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            user!.EmailConfirmed.Should().BeTrue();
        }
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

    private async Task ConfirmEmailAsync(string email)
    {
        string token;
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
        }

        var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", new { Email = email, Token = token });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
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

public record RegisterResponse(
    Guid Id,
    string? Email,
    string FirstName,
    string LastName,
    string Message);

public record ValidationProblem(Dictionary<string, string[]> Errors, int StatusCode);
