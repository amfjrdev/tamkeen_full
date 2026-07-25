using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SP.Infrastructure.Persistence;

namespace SP.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs periodically and hard-deletes refresh token rows that are both
/// expired AND revoked — keeping the RefreshTokens table lean.
/// </summary>
internal sealed class CleanupExpiredRefreshTokensJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanupExpiredRefreshTokensJob> _logger;
    private readonly TimeSpan _interval;

    public CleanupExpiredRefreshTokensJob(
        IServiceProvider serviceProvider,
        ILogger<CleanupExpiredRefreshTokensJob> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _interval = TimeSpan.FromHours(
            configuration.GetValue<int>("Jobs:CleanupRefreshTokensIntervalHours", 6));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Job}", nameof(CleanupExpiredRefreshTokensJob));
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Use raw SQL via EF to delete directly from the shadow table
        // without loading all users into memory — safe parameterized query.
        var cutoff = DateTime.UtcNow;
        var deleted = await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM RefreshTokens WHERE ExpiresAt < {cutoff} AND RevokedAt IS NOT NULL",
            cancellationToken);

        if (deleted > 0)
            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", deleted);
    }
}
