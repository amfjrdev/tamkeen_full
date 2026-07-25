// SP.Application/Abstractions/Messaging/INotificationService.cs

namespace SP.Application.Abstractions.Messaging;

public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
}
