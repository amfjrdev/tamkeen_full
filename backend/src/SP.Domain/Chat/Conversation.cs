using System;
using SP.Domain.Abstractions;

namespace SP.Domain.Chat;

public sealed class Conversation : AggregateRoot
{
    private Conversation() { }

    private Conversation(Guid id, Guid participant1Id, Guid participant2Id, int unlockCost) : base(id)
    {
        Participant1Id = participant1Id;
        Participant2Id = participant2Id;
        IsLocked = true;
        UnlockCost = unlockCost;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Participant1Id { get; private set; }
    public Guid Participant2Id { get; private set; }
    public bool IsLocked { get; private set; }
    public int UnlockCost { get; private set; }
    public string? LastMessageText { get; private set; }
    public Guid? LastMessageSenderId { get; private set; }
    public DateTime? LastMessageSentAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Conversation Create(Guid participant1Id, Guid participant2Id, int unlockCost = 5)
    {
        if (participant1Id == Guid.Empty)
            throw new ArgumentException("Participant 1 ID cannot be empty.", nameof(participant1Id));

        if (participant2Id == Guid.Empty)
            throw new ArgumentException("Participant 2 ID cannot be empty.", nameof(participant2Id));

        if (participant1Id == participant2Id)
            throw new ArgumentException("Participants must be different users.");

        return new Conversation(Guid.NewGuid(), participant1Id, participant2Id, unlockCost);
    }

    public void Unlock()
    {
        IsLocked = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastMessage(string text, Guid senderId, DateTime sentAt)
    {
        LastMessageText = text;
        LastMessageSenderId = senderId;
        LastMessageSentAt = sentAt;
        UpdatedAt = DateTime.UtcNow;
    }
}
