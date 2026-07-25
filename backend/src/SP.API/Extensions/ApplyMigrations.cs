using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SP.Infrastructure.Persistence;

namespace SP.API.Extensions;

public static class ApplyMigrations
{
    public static async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        const int maxRetries = 10;
        const int delaySeconds = 10;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Attempt {Attempt}/{Max}: Checking database connection...", attempt, maxRetries);
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");

                // Seed test data after successful migration
                await TestDataSeeder.SeedTestAccounts(dbContext, configuration);
                logger.LogInformation("Test data seeded successfully.");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex, "Migration attempt {Attempt} failed. Retrying in {Delay}s...", attempt, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "All {Max} migration attempts failed. Shutting down.", maxRetries);
                throw;
            }
        }
    }
}