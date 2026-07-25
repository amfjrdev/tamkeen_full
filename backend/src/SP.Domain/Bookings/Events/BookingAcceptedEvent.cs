using SP.Domain.Abstractions;

namespace SP.Domain.Bookings.Events;

public sealed record BookingAcceptedEvent(Guid BookingId, Guid ClientId, Guid ServiceId) : DomainEvent;
