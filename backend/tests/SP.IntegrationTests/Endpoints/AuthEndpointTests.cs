using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SP.IntegrationTests.Common;
using Xunit;

namespace SP.IntegrationTests.Endpoints;

public sealed class AuthEndpointTests : BaseIntegrationTest
{
    public AuthEndpointTests(IntegrationTestFactory factory) : base(factory) { }

    // ── Register ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "newuser@example.com",
            password = "Test@1234!",
            firstName = "New",
            lastName = "User",
            role = "Client"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadAsync<AuthResponse>(response);
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "dup@example.com",
            password = "Test@1234!",
            firstName = "A",
            lastName = "B",
            role = "Client"
        });

        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "dup@example.com",
            password = "Test@1234!",
            firstName = "A",
            lastName = "B",
            role = "Client"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "not-an-email",
            password = "Test@1234!",
            firstName = "A",
            lastName = "B",
            role = "Client"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidRole_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "user@example.com",
            password = "Test@1234!",
            firstName = "A",
            lastName = "B",
            role = "SuperAdmin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Login ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTokens()
    {
        await RegisterUserAsync("login@example.com");

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login@example.com",
            password = "Test@1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadAsync<AuthResponse>(response);
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        await RegisterUserAsync("wrongpw@example.com");

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "wrongpw@example.com",
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "ghost@example.com",
            password = "Test@1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Refresh ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_WithValidToken_Returns200WithNewTokens()
    {
        var loginResp = await LoginAsync("refresh@example.com");

        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = loginResp.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadAsync<AuthResponse>(response);
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBe(loginResp.RefreshToken); // rotated
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = "invalid-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithAlreadyUsedToken_Returns401()
    {
        var loginResp = await LoginAsync("reuse@example.com");

        // Use the token once
        await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = loginResp.RefreshToken
        });

        // Try to reuse it
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = loginResp.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Logout ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithValidToken_Returns204()
    {
        var loginResp = await LoginAsync("logout@example.com");
        SetAuthToken(loginResp.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/auth/logout", new
        {
            refreshToken = loginResp.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithoutAuth_Returns401()
    {
        ClearAuthToken();

        var response = await Client.PostAsJsonAsync("/api/auth/logout", new
        {
            refreshToken = "some-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task RegisterUserAsync(string email)
    {
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Test@1234!",
            firstName = "Test",
            lastName = "User",
            role = "Client"
        });
    }

    private async Task<AuthResponse> LoginAsync(string email)
    {
        await RegisterUserAsync(email);
        var resp = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Test@1234!"
        });
        return (await ReadAsync<AuthResponse>(resp))!;
    }

    private sealed record AuthResponse(
        string AccessToken,
        string RefreshToken,
        Guid UserId,
        string Email,
        string Role);
}
