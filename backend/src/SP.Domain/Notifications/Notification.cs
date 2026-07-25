using SP.Domain.Abstractions;

namespace SP.Domain.Notifications;

public sealed class Notification : Entity
{
    private Notification() { }
    
    private Notification(Guid id, Guid userId, string title, string message) : base(id)
    {
        UserId = userId;
        Title = title;
        Message = message;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public static Result<Notification> Create(Guid userId, string title, string message)
    {
        if (userId == Guid.Empty)
            return Result.Failure<Notification>(new Error("Notification.InvalidUserId", "User ID cannot be empty."));

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Notification>(new Error("Notification.InvalidTitle", "Title cannot be empty."));

        return Result.Success(new Notification(Guid.NewGuid(), userId, title.Trim(), message?.Trim() ?? string.Empty));
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}