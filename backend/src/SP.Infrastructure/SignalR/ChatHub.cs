using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Messaging;
using SP.Application.Chat.Commands.MarkMessageAsRead;
using SP.Application.Chat.Commands.SendMessage;
using SP.Application.Chat.Queries.GetConversationById;
using SP.Application.Chat.Queries.GetMessageById;

namespace SP.Infrastructure.SignalR;

[Authorize]
public sealed class ChatHub : Hub
{
    private readonly IPresenceService _presenceService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IDispatcher _dispatcher;

    public ChatHub(
        IPresenceService presenceService,
        ILogger<ChatHub> logger,
        IDispatcher dispatcher)
    {
        _presenceService = presenceService;
        _logger = logger;
        _dispatcher = dispatcher;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await _presenceService.SetOnlineAsync(userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await _presenceService.SetOfflineAsync(userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendTypingIndicator(Guid conversationId)
    {
        var senderId = GetUserId();

        var convResult = await _dispatcher.QueryAsync(new GetConversationByIdQuery(conversationId));
        if (convResult.IsFailure || convResult.Value is null) return;

        var conv = convResult.Value;
        var receiverId = conv.Participant1Id == senderId ? conv.Participant2Id : conv.Participant1Id;

        await _presenceService.SetTypingAsync(senderId, receiverId);
        await Clients.Group($"user-{receiverId}").SendAsync("UserTyping", new { SenderId = senderId });
    }

    public async Task SendMessage(Guid conversationId, string text)
    {
        var senderId = GetUserId();

        var result = await _dispatcher.SendAsync(new SendMessageCommand(senderId, conversationId, text));
        if (result.IsFailure)
            throw new HubException(result.Error.Message);

        var messageDto = result.Value;

        var convResult = await _dispatcher.QueryAsync(new GetConversationByIdQuery(conversationId));
        if (convResult.IsFailure || convResult.Value is null)
            throw new HubException("Conversation not found.");

        var conversation = convResult.Value;
        var receiverId = conversation.Participant1Id == senderId ? conversation.Participant2Id : conversation.Participant1Id;

        var messagePayload = new
        {
            id = messageDto.Id.ToString(),
            conversationId = conversationId.ToString(),
            senderId = messageDto.SenderId.ToString(),
            text = messageDto.Text,
            sentAt = messageDto.SentAt.ToString("o"),
            isRead = messageDto.IsRead
        };

        await Clients.Group($"user-{senderId}").SendAsync("ReceiveMessage", messagePayload);
        await Clients.Group($"user-{receiverId}").SendAsync("ReceiveMessage", messagePayload);

        _logger.LogInformation("Message sent in conversation {ConversationId} by user {UserId}", conversationId, senderId);
    }

    public async Task MessageRead(Guid messageId)
    {
        var userId = GetUserId();

        var msgResult = await _dispatcher.QueryAsync(new GetMessageByIdQuery(messageId));
        if (msgResult.IsFailure || msgResult.Value is null)
            throw new HubException("Message not found.");

        var message = msgResult.Value;

        var result = await _dispatcher.SendAsync(new MarkMessageAsReadCommand(userId, messageId));
        if (result.IsFailure)
            throw new HubException(result.Error.Message);

        await Clients.Group($"user-{message.SenderId}").SendAsync("MessageRead", new
        {
            messageId = message.Id.ToString(),
            readAt = DateTime.UtcNow.ToString("o")
        });
    }

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            throw new HubException("User not authenticated.");
        return userId;
    }
}
