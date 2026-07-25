// SP.Application/Notifications/Queries/GetNotifications/GetNotificationsQuery.cs

using SP.Application.Abstractions.Messaging;
using SP.Application.Notifications.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Notifications.Repositories;

namespace SP.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(Guid UserId) : IQuery<List<NotificationResponse>>;

public sealed class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, List<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<List<NotificationResponse>>> HandleAsync(
        GetNotificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        var response = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse(
                n.Id,
                n.UserId,
                n.Title,
                n.Message,
                n.IsRead,
                n.CreatedAt,
                n.ReadAt))
            .ToList();

        return Result.Success(response);
    }
}
