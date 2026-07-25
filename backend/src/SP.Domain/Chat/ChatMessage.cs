using System;
using SP.Domain.Abstractions;

namespace SP.Domain.Chat;

public sealed class ChatMessage : Entity
{
    private ChatMessage() { }

    private ChatMessage(Guid id, Guid conversationId, Guid senderId, string text) : base(id)
    {
        ConversationId = conversationId;
        SenderId = senderId;
        Text = text;
        SentAt = DateTime.UtcNow;
        IsRead = false;
    }

    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public bool IsRead { get; private set; }

    public static ChatMessage Create(Guid conversationId, Guid senderId, string text)
    {
        if (conversationId == Guid.Empty)
            throw new ArgumentException("Conversation ID cannot be empty.", nameof(conversationId));

        if (senderId == Guid.Empty)
            throw new ArgumentException("Sender ID cannot be empty.", nameof(senderId));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Message text cannot be empty.", nameof(text));

        return new ChatMessage(Guid.NewGuid(), conversationId, senderId, text.Trim());
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
