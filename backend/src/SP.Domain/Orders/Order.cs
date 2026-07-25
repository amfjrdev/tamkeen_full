namespace SP.Domain.Orders;

using SP.Domain.Orders.Enums;
using SP.Domain.Payments.Events;
using SP.Domain.Payments;
using SP.Domain.Abstractions;
using SP.Domain.Orders.Errors;
using SP.Domain.Payments.Errors;
public sealed class Order
{
    private readonly List<object> _domainEvents = [];

    public Guid Id { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Payment? Payment { get; private set; }

    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    private Order() { } 

    public static Order Create(Guid id, string customerEmail, decimal totalAmount, string currency)
    {
        return new Order
        {
            Id = id,
            CustomerEmail = customerEmail,
            TotalAmount = totalAmount,
            Currency = currency,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void InitiatePayment(Payment payment)
    {
        Payment = payment;
        Status = OrderStatus.PaymentPending;
        UpdatedAt = DateTime.UtcNow;
    }

  public Result ConfirmPayment()
{
    if (Payment is null)
        return Result.Failure(OrderErrors.PaymentNotFound);

    Payment.MarkAsPaid();

    Status = OrderStatus.Paid;
    UpdatedAt = DateTime.UtcNow;

    _domainEvents.Add(new PaymentCompletedEvent(
        Id,
        Payment.Id,
        Payment.Amount,
        Payment.Currency,
        DateTime.UtcNow));

    return Result.Success();
}
public Result FailPayment()
{
    if (Payment is null)
        return Result.Failure(OrderErrors.PaymentNotFound);

    Payment.MarkAsFailed();

    Status = OrderStatus.Failed;
    UpdatedAt = DateTime.UtcNow;

    _domainEvents.Add(new PaymentFailedEvent(
        Id,
        Payment.Id,
        DateTime.UtcNow));

    return Result.Success();
}

    public void ClearDomainEvents() => _domainEvents.Clear();
}