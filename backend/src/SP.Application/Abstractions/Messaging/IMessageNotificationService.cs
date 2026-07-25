namespace SP.Application.Abstractions.Messaging;

public interface IMessageNotificationService
{
    Task NotifyMessageReceivedAsync(Guid senderId, Guid receiverId, object messagePayload, CancellationToken cancellationToken = default);
    Task NotifyMessageDeliveredAsync(Guid senderId, Guid messageId, CancellationToken cancellationToken = default);
    Task NotifyMessageReadAsync(Guid senderId, Guid messageId, DateTime readAt, CancellationToken cancellationToken = default);
    Task NotifyUserTypingAsync(Guid senderId, Guid receiverId, CancellationToken cancellationToken = default);
}
