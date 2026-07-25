// SP.Application/Notifications/Dtos/NotificationResponse.cs

namespace SP.Application.Notifications.Dtos;

public sealed record NotificationResponse(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);
