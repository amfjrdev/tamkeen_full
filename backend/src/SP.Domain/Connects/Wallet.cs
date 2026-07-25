using System;
using SP.Domain.Abstractions;

namespace SP.Domain.Connects;

public sealed class Wallet : AggregateRoot
{
    private Wallet() { }

    private Wallet(Guid id, Guid userId, int balance) : base(id)
    {
        UserId = userId;
        Balance = balance;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public int Balance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public static Wallet Create(Guid userId, int initialBalance = 0)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new Wallet(Guid.NewGuid(), userId, initialBalance);
    }

    public Result Credit(int amount)
    {
        if (amount <= 0)
            return Result.Failure(new Error("Wallet.InvalidAmount", "Credit amount must be positive."));

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
        Version++;
        return Result.Success();
    }

    public Result Debit(int amount)
    {
        if (amount <= 0)
            return Result.Failure(new Error("Wallet.InvalidAmount", "Debit amount must be positive."));

        if (Balance < amount)
            return Result.Failure(new Error("Wallet.InsufficientBalance", "Insufficient connects balance."));

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
        Version++;
        return Result.Success();
    }
}
