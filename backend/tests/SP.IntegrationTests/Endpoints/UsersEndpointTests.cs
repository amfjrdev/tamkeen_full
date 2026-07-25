using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SP.IntegrationTests.Common;
using Xunit;

namespace SP.IntegrationTests.Endpoints;

public sealed class UsersEndpointTests : BaseIntegrationTest
{
    public UsersEndpointTests(IntegrationTestFactory factory) : base(factory) { }

    // ── GET /api/users/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task GetUserById_WhenExists_Returns200()
    {
        var token = await RegisterAndLoginAsync("getuser@example.com");
        SetAuthToken(token);

        var meResp = await Client.GetAsync("/api/users/me");
        // fallback: parse userId from register response
        var registerResp = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "getuser2@example.com", password = "Test@1234!",
            firstName = "A", lastName = "B", role = "Client"
        });
        var body = await ReadAsync<AuthBody>(registerResp);

        var response = await Client.GetAsync($"/api/users/{body!.UserId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserById_WhenNotFound_Returns404()
    {
        var token = await RegisterAndLoginAsync("getuser3@example.com");
        SetAuthToken(token);

        var response = await Client.GetAsync($"/api/users/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserById_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync($"/api/users/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/users (admin only) ───────────────────────────────────────

    [Fact]
    public async Task GetAllUsers_AsAdmin_Returns200()
    {
        var token = await RegisterAndLoginAsync("admin@users.com", role: "Admin");
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllUsers_AsClient_Returns403()
    {
        var token = await RegisterAndLoginAsync("client@users.com");
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── PUT /api/users/profile ────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_WithValidData_Returns204()
    {
        var token = await RegisterAndLoginAsync("profile@users.com");
        SetAuthToken(token);

        var response = await Client.PutAsJsonAsync("/api/users/profile", new
        {
            firstName = "Updated",
            lastName = "Name",
            phoneNumber = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProfile_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.PutAsJsonAsync("/api/users/profile", new
        {
            firstName = "X", lastName = "Y", phoneNumber = (string?)null
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PATCH /api/users/phone ────────────────────────────────────────────

    [Fact]
    public async Task UpdatePhone_WithValidPhone_Returns204()
    {
        var token = await RegisterAndLoginAsync("phone@users.com");
        SetAuthToken(token);

        var response = await Client.PatchAsJsonAsync("/api/users/phone", new
        {
            phoneNumber = "+1234567890"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdatePhone_WithInvalidPhone_Returns400()
    {
        var token = await RegisterAndLoginAsync("phone2@users.com");
        SetAuthToken(token);

        var response = await Client.PatchAsJsonAsync("/api/users/phone", new
        {
            phoneNumber = "not-a-phone"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Admin actions ─────────────────────────────────────────────────────

    [Fact]
    public async Task BlockAndUnblock_User_Works()
    {
        var adminToken = await RegisterAndLoginAsync("admin2@users.com", role: "Admin");
        var targetResp = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "target@users.com", password = "Test@1234!",
            firstName = "T", lastName = "U", role = "Client"
        });
        var target = await ReadAsync<AuthBody>(targetResp);

        SetAuthToken(adminToken);

        var blockResp = await Client.PostAsJsonAsync($"/api/users/{target!.UserId}/block", new { reason = "spam" });
        blockResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var unblockResp = await Client.PostAsJsonAsync($"/api/users/{target.UserId}/unblock", new { });
        unblockResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task BlockUser_WhenAlreadyBlocked_Returns409()
    {
        var adminToken = await RegisterAndLoginAsync("admin3@users.com", role: "Admin");
        var targetResp = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "target2@users.com", password = "Test@1234!",
            firstName = "T", lastName = "U", role = "Client"
        });
        var target = await ReadAsync<AuthBody>(targetResp);

        SetAuthToken(adminToken);
        await Client.PostAsJsonAsync($"/api/users/{target!.UserId}/block", new { reason = "spam" });

        var response = await Client.PostAsJsonAsync($"/api/users/{target.UserId}/block", new { reason = "again" });
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SuspendAndUnsuspend_User_Works()
    {
        var adminToken = await RegisterAndLoginAsync("admin4@users.com", role: "Admin");
        var targetResp = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "target3@users.com", password = "Test@1234!",
            firstName = "T", lastName = "U", role = "Client"
        });
        var target = await ReadAsync<AuthBody>(targetResp);

        SetAuthToken(adminToken);

        var suspendResp = await Client.PostAsJsonAsync($"/api/users/{target!.UserId}/suspend", new { reason = "violation" });
        suspendResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var unsuspendResp = await Client.PostAsJsonAsync($"/api/users/{target.UserId}/unsuspend", new { });
        unsuspendResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AdminActions_AsClient_Returns403()
    {
        var clientToken = await RegisterAndLoginAsync("client2@users.com");
        var targetResp = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "target4@users.com", password = "Test@1234!",
            firstName = "T", lastName = "U", role = "Client"
        });
        var target = await ReadAsync<AuthBody>(targetResp);

        SetAuthToken(clientToken);
        var response = await Client.PostAsJsonAsync($"/api/users/{target!.UserId}/block", new { reason = "x" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task BlockUser_NonExistentUser_Returns404()
    {
        var adminToken = await RegisterAndLoginAsync("admin5@users.com", role: "Admin");
        SetAuthToken(adminToken);

        var response = await Client.PostAsJsonAsync($"/api/users/{Guid.NewGuid()}/block", new { reason = "x" });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/users ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAccount_WithCorrectConfirmation_Returns204()
    {
        var token = await RegisterAndLoginAsync("delete@users.com");
        SetAuthToken(token);

        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/users")
        {
            Content = JsonContent.Create(new { confirmationText = "DELETE", password = "Test@1234!" })
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAccount_WithWrongConfirmation_Returns400()
    {
        var token = await RegisterAndLoginAsync("delete2@users.com");
        SetAuthToken(token);

        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/users")
        {
            Content = JsonContent.Create(new { confirmationText = "WRONG", password = "Test@1234!" })
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record AuthBody(string AccessToken, string RefreshToken, Guid UserId, string Email, string Role);
}
