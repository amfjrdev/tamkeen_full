using System;
using SP.Domain.Abstractions;
using SP.Domain.Payments.Enums;
using SP.Domain.Payments.Errors;

namespace SP.Domain.Payments;

public sealed class Payment : AggregateRoot
{
    private Payment() { }

    private Payment(
        Guid id,
        Guid userId,
        Guid? orderId,
        string checkoutId,
        string? checkoutUrl,
        string? packId,
        decimal amount,
        string currency,
        PaymentStatus status,
        string provider)
        : base(id)
    {
        UserId = userId;
        OrderId = orderId;
        CheckoutId = checkoutId;
        CheckoutUrl = checkoutUrl;
        PackId = packId;
        Amount = amount;
        Currency = currency;
        Status = status;
        Provider = provider;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid? OrderId { get; private set; }
    public string CheckoutId { get; private set; } = string.Empty;
    public string? CheckoutUrl { get; private set; }
    public string? PackId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int Version { get; private set; }

    public string ExternalCheckoutId => CheckoutId;

    public static Payment Create(
        Guid userId,
        string checkoutId,
        string? checkoutUrl,
        string packId,
        decimal amount,
        string currency = "DZD",
        string provider = "Chargily")
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(checkoutId))
            throw new ArgumentException("Checkout ID cannot be empty.", nameof(checkoutId));

        if (string.IsNullOrWhiteSpace(packId))
            throw new ArgumentException("Package ID cannot be empty.", nameof(packId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        return new Payment(
            Guid.NewGuid(),
            userId,
            null,
            checkoutId,
            checkoutUrl,
            packId,
            amount,
            currency,
            PaymentStatus.Pending,
            provider);
    }

    public static Payment CreatePending(
        Guid orderId,
        string externalCheckoutId,
        string checkoutUrl,
        decimal amount,
        string currency)
    {
        return new Payment(
            Guid.NewGuid(),
            Guid.Empty,
            orderId,
            externalCheckoutId,
            checkoutUrl,
            null,
            amount,
            currency,
            PaymentStatus.Pending,
            "Chargily");
    }

    public Result MarkAsPaid()
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(PaymentErrors.InvalidStatusForPaid);

        Status = PaymentStatus.Paid;
        CompletedAt = DateTime.UtcNow;
        Version++;
        return Result.Success();
    }

    public Result MarkAsFailed()
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(PaymentErrors.InvalidStatusForFailed);

        Status = PaymentStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        Version++;
        return Result.Success();
    }

    public Result Complete(PaymentStatus newStatus)
    {
        if (Status != PaymentStatus.Pending)
        {
            return Result.Failure(new Error("Payment.NotPending", "Only pending payments can be completed."));
        }

        if (newStatus == PaymentStatus.Pending)
        {
            return Result.Failure(new Error("Payment.InvalidStatus", "Cannot transition to Pending status."));
        }

        Status = newStatus;
        CompletedAt = DateTime.UtcNow;
        Version++;
        return Result.Success();
    }
}
