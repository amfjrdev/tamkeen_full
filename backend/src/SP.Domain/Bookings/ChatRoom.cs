using SP.Domain.Abstractions;

namespace SP.Domain.Bookings;

public sealed class ChatRoom : Entity
{
    private readonly List<ChatMessage> _messages = [];

    private ChatRoom() { }

    private ChatRoom(Guid id, Guid bookingId) : base(id)
    {
        BookingId = bookingId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid BookingId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    internal static ChatRoom Create(Guid bookingId)
        => new(Guid.NewGuid(), bookingId);

    internal ChatMessage AddMessage(Guid senderId, string content)
    {
        var message = ChatMessage.Create(Id, senderId, content);
        _messages.Add(message);
        return message;
    }
}
