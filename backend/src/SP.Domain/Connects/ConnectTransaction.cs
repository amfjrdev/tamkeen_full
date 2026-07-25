using System;
using SP.Domain.Abstractions;

namespace SP.Domain.Connects;

public sealed class ConnectTransaction : Entity
{
    private ConnectTransaction() { }

    private ConnectTransaction(
        Guid id,
        Guid userId,
        int amount,
        string type,
        string? idempotencyKey,
        Guid? referenceId) : base(id)
    {
        UserId = userId;
        Amount = amount;
        Type = type;
        IdempotencyKey = idempotencyKey;
        ReferenceId = referenceId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public int Amount { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string? IdempotencyKey { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static ConnectTransaction Create(
        Guid userId,
        int amount,
        string type,
        string? idempotencyKey,
        Guid? referenceId = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Transaction type cannot be empty.", nameof(type));

        return new ConnectTransaction(Guid.NewGuid(), userId, amount, type, idempotencyKey, referenceId);
    }
}
