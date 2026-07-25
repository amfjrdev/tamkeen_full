using SP.Domain.Abstractions;

namespace SP.Domain.Bookings;

public sealed class ChatMessage : Entity
{
    private ChatMessage() { }

    private ChatMessage(Guid id, Guid chatRoomId, Guid senderId, string content) : base(id)
    {
        ChatRoomId = chatRoomId;
        SenderId = senderId;
        Content = content;
        SentAt = DateTime.UtcNow;
        IsRead = false;
    }

    public Guid ChatRoomId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public bool IsRead { get; private set; }

    internal static ChatMessage Create(Guid chatRoomId, Guid senderId, string content)
        => new(Guid.NewGuid(), chatRoomId, senderId, content);

    internal void MarkAsRead() => IsRead = true;
}
