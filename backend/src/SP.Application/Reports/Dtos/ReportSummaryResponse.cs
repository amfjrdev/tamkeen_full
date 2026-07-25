// SP.Application/Reports/Dtos/ReportResponses.cs

namespace SP.Application.Reports.Dtos;

public sealed record ReportSummaryResponse(
    Guid Id,
    Guid BookingId,
    Guid ReporterId,
    string Reason,
    string Status,
    DateTime CreatedAt);
