using SP.Domain.Abstractions;

namespace SP.Domain.Bookings.Events;

public sealed record MessageSentEvent(Guid BookingId, Guid MessageId, Guid SenderId) : DomainEvent;
