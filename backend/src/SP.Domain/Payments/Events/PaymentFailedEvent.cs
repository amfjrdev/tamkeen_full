using System;
using SP.Domain.Abstractions;

namespace SP.Domain.Payments.Events;

public sealed record PaymentFailedEvent(
    Guid OrderId,
    Guid PaymentId,
    DateTime FailedAt
) : DomainEvent;
