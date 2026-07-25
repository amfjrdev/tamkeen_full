using SP.Domain.Abstractions;

namespace SP.Domain.Bookings.Events;

public sealed record BookingCompletedEvent(Guid BookingId, Guid ClientId, Guid ServiceId) : DomainEvent;
