using Microsoft.AspNetCore.SignalR;
using SP.Application.Abstractions.Messaging;

namespace SP.Infrastructure.SignalR;

internal sealed class MessageNotificationService : IMessageNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public MessageNotificationService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMessageReceivedAsync(
        Guid senderId, Guid receiverId, object messagePayload, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"user-{senderId}").SendAsync("ReceiveMessage", messagePayload, cancellationToken);
        await _hubContext.Clients.Group($"user-{receiverId}").SendAsync("ReceiveMessage", messagePayload, cancellationToken);
    }

    public async Task NotifyMessageDeliveredAsync(
        Guid senderId, Guid messageId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"user-{senderId}")
            .SendAsync("MessageDelivered", new { MessageId = messageId }, cancellationToken);
    }

    public async Task NotifyMessageReadAsync(
        Guid senderId, Guid messageId, DateTime readAt, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"user-{senderId}")
            .SendAsync("MessageRead", new { MessageId = messageId, ReadAt = readAt }, cancellationToken);
    }

    public async Task NotifyUserTypingAsync(
        Guid senderId, Guid receiverId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"user-{receiverId}")
            .SendAsync("UserTyping", new { SenderId = senderId }, cancellationToken);
    }
}
