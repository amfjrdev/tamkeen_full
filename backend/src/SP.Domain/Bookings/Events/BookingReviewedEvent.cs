using SP.Domain.Abstractions;

namespace SP.Domain.Bookings.Events;

public sealed record BookingReviewedEvent(Guid BookingId, Guid ServiceId, int Rating) : DomainEvent;
