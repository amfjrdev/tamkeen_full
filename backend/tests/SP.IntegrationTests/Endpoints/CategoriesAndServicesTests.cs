using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SP.IntegrationTests.Common;
using Xunit;

namespace SP.IntegrationTests.Endpoints;

public sealed class CategoriesEndpointTests : BaseIntegrationTest
{
    public CategoriesEndpointTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAllCategories_ReturnsOk()
    {
        var response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCategory_AsAdmin_Returns201()
    {
        var token = await RegisterAndLoginAsync("admin@cat.com", "Test@1234!", "Admin");
        SetAuthToken(token);

        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = "Plumbing",
            description = "Plumbing services"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCategory_AsClient_Returns403()
    {
        var token = await RegisterAndLoginAsync("client@cat.com", "Test@1234!", "Client");
        SetAuthToken(token);

        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = "Plumbing",
            description = "desc"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_Returns409()
    {
        var token = await RegisterAndLoginAsync("admin2@cat.com", "Test@1234!", "Admin");
        SetAuthToken(token);

        await Client.PostAsJsonAsync("/api/categories", new { name = "Electrical", description = "desc" });
        var response = await Client.PostAsJsonAsync("/api/categories", new { name = "Electrical", description = "desc" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCategory_WithoutAuth_Returns401()
    {
        ClearAuthToken();

        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = "Plumbing",
            description = "desc"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCategory_AsAdmin_Returns204()
    {
        var token = await RegisterAndLoginAsync("admin3@cat.com", "Test@1234!", "Admin");
        SetAuthToken(token);

        var createResp = await Client.PostAsJsonAsync("/api/categories", new { name = "ToDelete", description = "desc" });
        var id = (await ReadAsync<IdResponse>(createResp))!.Id;

        var deleteResp = await Client.DeleteAsync($"/api/categories/{id}");

        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetCategoryById_WhenExists_Returns200()
    {
        var token = await RegisterAndLoginAsync("admin4@cat.com", "Test@1234!", "Admin");
        SetAuthToken(token);

        var createResp = await Client.PostAsJsonAsync("/api/categories", new { name = "Carpentry", description = "desc" });
        var id = (await ReadAsync<IdResponse>(createResp))!.Id;

        ClearAuthToken();
        var getResp = await Client.GetAsync($"/api/categories/{id}");

        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategoryById_WhenNotFound_Returns404()
    {
        var response = await Client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record IdResponse(Guid Id);
}

public sealed class ServicesEndpointTests : BaseIntegrationTest
{
    public ServicesEndpointTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAllServices_IsPublic_Returns200()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateService_AsProvider_Returns201()
    {
        var (categoryId, providerToken) = await SetupProviderWithCategoryAsync();
        SetAuthToken(providerToken);

        var response = await Client.PostAsJsonAsync("/api/services", new
        {
            categoryId,
            name = "Pipe Repair",
            description = "Fix leaking pipes",
            price = 75.00,
            durationMinutes = 60
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateService_AsClient_Returns403()
    {
        var (categoryId, _) = await SetupProviderWithCategoryAsync();
        var clientToken = await RegisterAndLoginAsync("client@svc.com", "Test@1234!", "Client");
        SetAuthToken(clientToken);

        var response = await Client.PostAsJsonAsync("/api/services", new
        {
            categoryId,
            name = "Pipe Repair",
            description = "desc",
            price = 75.00,
            durationMinutes = 60
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateService_ThenActivate_Works()
    {
        var (categoryId, providerToken) = await SetupProviderWithCategoryAsync("p2@svc.com");
        SetAuthToken(providerToken);

        var svcResp = await Client.PostAsJsonAsync("/api/services", new
        {
            categoryId,
            name = "Wiring",
            description = "desc",
            price = 100.0,
            durationMinutes = 90
        });
        var svcId = (await ReadAsync<IdResponse>(svcResp))!.Id;

        var deactivateResp = await Client.PatchAsync($"/api/services/{svcId}/deactivate", null);
        deactivateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var activateResp = await Client.PatchAsync($"/api/services/{svcId}/activate", null);
        activateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<(Guid CategoryId, string ProviderToken)> SetupProviderWithCategoryAsync(
        string providerEmail = "provider@svc.com")
    {
        var adminToken = await RegisterAndLoginAsync($"admin_{providerEmail}", "Test@1234!", "Admin");
        SetAuthToken(adminToken);
        var catResp = await Client.PostAsJsonAsync("/api/categories", new { name = $"Cat_{Guid.NewGuid()}", description = "desc" });
        var catId = (await ReadAsync<IdResponse>(catResp))!.Id;

        var providerToken = await RegisterAndLoginAsync(providerEmail, "Test@1234!", "Provider");
        return (catId, providerToken);
    }

    private sealed record IdResponse(Guid Id);
}
