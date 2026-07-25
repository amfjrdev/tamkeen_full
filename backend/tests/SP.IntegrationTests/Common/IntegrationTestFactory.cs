using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SP.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Xunit;

namespace SP.IntegrationTests.Common;

/// <summary>
/// Shared fixture that spins up real SQL Server + Redis containers once per test collection.
/// </summary>
public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string SqlContainerPassword = "Test@Strong!Pass1";

    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword(SqlContainerPassword)
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        await _redisContainer.StartAsync();
        Client = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        await _redisContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace real DB with test container DB
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(_sqlContainer.GetConnectionString(), sql =>
                {
                    sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                }));

            // Replace Redis connection string
            var redisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
            if (redisDescriptor is not null)
                services.Remove(redisDescriptor);

            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
                StackExchange.Redis.ConnectionMultiplexer.Connect(
                    _redisContainer.GetConnectionString()));

            // Apply migrations
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        });

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-test-secret-key-minimum-32-chars!",
                ["Jwt:Issuer"] = "SP.API",
                ["Jwt:Audience"] = "SP.Client",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["Smtp:Enabled"] = "false"
            });
        });
    }

    /// <summary>Resets the database between tests by truncating all tables.</summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Truncate in dependency order
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ChatMessages");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ChatRooms");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Reviews");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Reports");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Bookings");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Notifications");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ProjectImages");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Projects");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Portfolios");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Services");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Categories");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM RefreshTokens");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM UserCredentials");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Users");
    }
}
