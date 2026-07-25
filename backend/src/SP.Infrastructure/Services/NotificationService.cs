// SP.Infrastructure/Services/NotificationService.cs

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Notifications;
using SP.Domain.Notifications.Repositories;
using SP.Infrastructure.SignalR;

namespace SP.Infrastructure.Services;

internal sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IHubContext<ChatHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendAsync(
        Guid userId,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        var result = Notification.Create(userId, title, message);
        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Failed to create notification for user {UserId}: {Error}",
                userId, result.Error.Message);
            return;
        }

        await _notificationRepository.AddAsync(result.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Push real-time to the user's SignalR group
        await _hubContext.Clients
            .Group($"user-{userId}")
            .SendAsync("ReceiveNotification", new
            {
                result.Value.Id,
                result.Value.Title,
                result.Value.Message,
                result.Value.CreatedAt
            }, cancellationToken);

        _logger.LogInformation(
            "Notification sent to user {UserId}: {Title}", userId, title);
    }
}
