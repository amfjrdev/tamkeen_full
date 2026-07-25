using SP.Domain.Abstractions;

namespace SP.Domain.Bookings;

public sealed class Report : Entity
{
    private Report() { }

    private Report(Guid id, Guid bookingId, Guid reporterId, string reason) : base(id)
    {
        BookingId = bookingId;
        ReporterId = reporterId;
        Reason = reason;
        Status = ReportStatus.Open;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid BookingId { get; private set; }
    public Guid ReporterId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public ReportStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    internal static Report Create(Guid bookingId, Guid reporterId, string reason)
        => new(Guid.NewGuid(), bookingId, reporterId, reason);

    internal void Resolve() => Status = ReportStatus.Resolved;
}
