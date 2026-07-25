using SP.Domain.Abstractions;

namespace SP.Domain.Bookings.Events;

public sealed record BookingReportedEvent(Guid BookingId, Guid ReportId, Guid ReporterId) : DomainEvent;
