using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SP.IntegrationTests.Common;
using Xunit;

namespace SP.IntegrationTests.Endpoints;

public sealed class PortfolioEndpointTests : BaseIntegrationTest
{
    public PortfolioEndpointTests(IntegrationTestFactory factory) : base(factory) { }

    // ── POST /api/portfolio/projects ──────────────────────────────────────

    [Fact]
    public async Task AddProject_AsProvider_Returns201()
    {
        var token = await RegisterAndLoginAsync("provider@portfolio.com", role: "Provider");
        SetAuthToken(token);

        var response = await Client.PostAsJsonAsync("/api/portfolio/projects", new
        {
            name = "My Project",
            description = "A great project",
            imageUrls = new List<string> { "https://example.com/img1.jpg" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadAsync<IdResponse>(response);
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddProject_AsClient_Returns403()
    {
        var token = await RegisterAndLoginAsync("client@portfolio.com");
        SetAuthToken(token);

        var response = await Client.PostAsJsonAsync("/api/portfolio/projects", new
        {
            name = "My Project",
            description = "desc",
            imageUrls = new List<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddProject_WithoutAuth_Returns401()
    {
        ClearAuthToken();

        var response = await Client.PostAsJsonAsync("/api/portfolio/projects", new
        {
            name = "My Project",
            description = "desc",
            imageUrls = new List<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/portfolio/projects/{projectId} ────────────────────────

    [Fact]
    public async Task RemoveProject_OwnProject_Returns204()
    {
        var token = await RegisterAndLoginAsync("provider2@portfolio.com", role: "Provider");
        SetAuthToken(token);

        var addResp = await Client.PostAsJsonAsync("/api/portfolio/projects", new
        {
            name = "To Remove",
            description = "desc",
            imageUrls = new List<string>()
        });
        var projectId = (await ReadAsync<IdResponse>(addResp))!.Id;

        var deleteResp = await Client.DeleteAsync($"/api/portfolio/projects/{projectId}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveProject_NonExistent_Returns404()
    {
        var token = await RegisterAndLoginAsync("provider3@portfolio.com", role: "Provider");
        SetAuthToken(token);

        var response = await Client.DeleteAsync($"/api/portfolio/projects/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveProject_OtherProviderProject_Returns403()
    {
        var provider1Token = await RegisterAndLoginAsync("prov1@portfolio.com", role: "Provider");
        var provider2Token = await RegisterAndLoginAsync("prov2@portfolio.com", role: "Provider");

        SetAuthToken(provider1Token);
        var addResp = await Client.PostAsJsonAsync("/api/portfolio/projects", new
        {
            name = "P1 Project",
            description = "desc",
            imageUrls = new List<string>()
        });
        var projectId = (await ReadAsync<IdResponse>(addResp))!.Id;

        SetAuthToken(provider2Token);
        var response = await Client.DeleteAsync($"/api/portfolio/projects/{projectId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/portfolio/{providerId} ───────────────────────────────────

    [Fact]
    public async Task GetPortfolio_IsPublic_Returns200()
    {
        var registerResp = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "prov4@portfolio.com", password = "Test@1234!",
            firstName = "P", lastName = "R", role = "Provider"
        });
        var body = await ReadAsync<AuthBody>(registerResp);

        ClearAuthToken();
        var response = await Client.GetAsync($"/api/portfolio/{body!.UserId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPortfolio_NonExistentProvider_Returns404()
    {
        ClearAuthToken();
        var response = await Client.GetAsync($"/api/portfolio/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record IdResponse(Guid Id);
    private sealed record AuthBody(string AccessToken, string RefreshToken, Guid UserId, string Email, string Role);
}

public sealed class NotificationsEndpointTests : BaseIntegrationTest
{
    public NotificationsEndpointTests(IntegrationTestFactory factory) : base(factory) { }

    // ── GET /api/notifications ────────────────────────────────────────────

    [Fact]
    public async Task GetNotifications_Authenticated_Returns200()
    {
        var token = await RegisterAndLoginAsync("notif@example.com");
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/notifications");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNotifications_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/notifications");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/notifications/{id}/read ─────────────────────────────────

    [Fact]
    public async Task MarkAsRead_NonExistentNotification_Returns404()
    {
        var token = await RegisterAndLoginAsync("notif2@example.com");
        SetAuthToken(token);

        var response = await Client.PostAsJsonAsync($"/api/notifications/{Guid.NewGuid()}/read", new { });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAsRead_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.PostAsJsonAsync($"/api/notifications/{Guid.NewGuid()}/read", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public sealed class ReportsEndpointTests : BaseIntegrationTest
{
    public ReportsEndpointTests(IntegrationTestFactory factory) : base(factory) { }

    // ── GET /api/reports ──────────────────────────────────────────────────

    [Fact]
    public async Task GetReports_AsAdmin_Returns200()
    {
        var token = await RegisterAndLoginAsync("admin@reports.com", role: "Admin");
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/reports");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetReports_AsClient_Returns403()
    {
        var token = await RegisterAndLoginAsync("client@reports.com");
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/reports");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetReports_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/reports");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReports_WithStatusFilter_Returns200()
    {
        var token = await RegisterAndLoginAsync("admin2@reports.com", role: "Admin");
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/reports?status=Pending&page=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetReports_WithInvalidStatusFilter_Returns200WithEmptyOrAll()
    {
        // Invalid enum value should be treated as null (no filter) — not a 500
        var token = await RegisterAndLoginAsync("admin3@reports.com", role: "Admin");
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/reports?status=InvalidStatus");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── POST /api/reports/{id}/resolve ────────────────────────────────────

    [Fact]
    public async Task ResolveReport_NonExistent_Returns404()
    {
        var token = await RegisterAndLoginAsync("admin4@reports.com", role: "Admin");
        SetAuthToken(token);

        var response = await Client.PostAsJsonAsync($"/api/reports/{Guid.NewGuid()}/resolve", new { });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResolveReport_AsClient_Returns403()
    {
        var token = await RegisterAndLoginAsync("client2@reports.com");
        SetAuthToken(token);

        var response = await Client.PostAsJsonAsync($"/api/reports/{Guid.NewGuid()}/resolve", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ResolveReport_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.PostAsJsonAsync($"/api/reports/{Guid.NewGuid()}/resolve", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResolveReport_FullFlow_Returns204()
    {
        // Setup: create a booking with a report, then resolve it
        var clientToken = await RegisterAndLoginAsync("rc@reports.com", role: "Client");
        var providerToken = await RegisterAndLoginAsync("rp@reports.com", role: "Provider");
        var adminToken = await RegisterAndLoginAsync("ra@reports.com", role: "Admin");

        SetAuthToken(adminToken);
        var catResp = await Client.PostAsJsonAsync("/api/categories", new { name = "ReportCat", description = "d" });
        var catId = (await ReadAsync<IdResponse>(catResp))!.Id;

        SetAuthToken(providerToken);
        var svcResp = await Client.PostAsJsonAsync("/api/services", new
        {
            categoryId = catId, name = "ReportSvc", description = "d", price = 10.0, durationMinutes = 30
        });
        var svcId = (await ReadAsync<IdResponse>(svcResp))!.Id;

        SetAuthToken(clientToken);
        var bookResp = await Client.PostAsJsonAsync("/api/bookings", new
        {
            serviceId = svcId, scheduledDate = DateTime.UtcNow.AddDays(3)
        });
        var bookingId = (await ReadAsync<IdResponse>(bookResp))!.Id;

        SetAuthToken(providerToken);
        await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/accept", new { });
        await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/complete", new { });

        SetAuthToken(clientToken);
        await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/report", new { reason = "Late arrival" });

        // Admin resolves the report
        SetAuthToken(adminToken);
        var reportsResp = await Client.GetAsync("/api/reports");
        var reports = await ReadAsync<PagedResponse<ReportItem>>(reportsResp);
        var reportId = reports!.Items.First().Id;

        var resolveResp = await Client.PostAsJsonAsync($"/api/reports/{reportId}/resolve", new { });
        resolveResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed record IdResponse(Guid Id);
    private sealed record ReportItem(Guid Id, string Status);
    private sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
}
