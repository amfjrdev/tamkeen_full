using System;
using SP.Domain.Abstractions;

namespace SP.Domain.Payments.Events;

public sealed record PaymentCompletedEvent(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    DateTime PaidAt
) : DomainEvent;
