using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SP.Application.Abstractions.Authentication;
using SP.Domain.Users;
using SP.Infrastructure.Authentication;
using SP.Infrastructure.Persistence;
using Xunit;

namespace SP.IntegrationTests.Common;

[Collection("Integration")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly IntegrationTestFactory Factory;
    protected readonly HttpClient Client;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected BaseIntegrationTest(IntegrationTestFactory factory)
    {
        Factory = factory;
        Client = factory.Client;
    }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // ── Auth helpers ──────────────────────────────────────────────────────

    protected async Task<string> RegisterAndLoginAsync(
        string email = "test@example.com",
        string password = "Test@1234!",
        string role = "Client")
    {
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password,
            firstName = "Test",
            lastName = "User",
            role
        });

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponseBody>(JsonOptions);
        return body!.AccessToken;
    }

    protected void SetAuthToken(string token)
        => Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    protected void ClearAuthToken()
        => Client.DefaultRequestHeaders.Authorization = null;

    // ── DB helpers ────────────────────────────────────────────────────────

    protected ApplicationDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    // ── Response helpers ──────────────────────────────────────────────────

    protected static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<T>(JsonOptions);

    private sealed record AuthResponseBody(
        string AccessToken,
        string RefreshToken,
        Guid UserId,
        string Email,
        string Role);
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationTestFactory> { }
