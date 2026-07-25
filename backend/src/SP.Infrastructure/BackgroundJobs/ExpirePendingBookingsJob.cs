using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Repositories;

namespace SP.Infrastructure.BackgroundJobs;

internal sealed class ExpirePendingBookingsJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpirePendingBookingsJob> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly TimeSpan _expirationTimeout;

    public ExpirePendingBookingsJob(
        IServiceProvider serviceProvider,
        ILogger<ExpirePendingBookingsJob> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _checkInterval = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Jobs:ExpireBookingsIntervalMinutes", 15));
        _expirationTimeout = TimeSpan.FromHours(
            configuration.GetValue<int>("Jobs:BookingExpirationHours", 24));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in {Job}", nameof(ExpirePendingBookingsJob));
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ExpireBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetService<INotificationService>();

        var pendingBookings = await bookingRepository.GetPendingBookingsAsync(cancellationToken);
        var expired = 0;

        foreach (var booking in pendingBookings)
        {
            if (!booking.IsExpired(_expirationTimeout))
                continue;

            var result = booking.Expire();
            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "Could not expire booking {BookingId}: {Error}",
                    booking.Id, result.Error.Message);
                continue;
            }

            bookingRepository.Update(booking);
            expired++;

            if (notificationService is not null)
            {
                await notificationService.SendAsync(
                    booking.ClientId,
                    "Booking Expired",
                    "Your booking request has expired because it was not accepted in time.",
                    cancellationToken);
            }
        }

        if (expired > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Expired {Count} pending bookings", expired);
        }
    }
}
