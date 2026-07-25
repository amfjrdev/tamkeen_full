using SP.Domain.Abstractions;

namespace SP.Domain.Bookings.Events;

public sealed record BookingRejectedEvent(Guid BookingId, Guid ClientId) : DomainEvent;
