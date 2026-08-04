// SP.Application/Bookings/Dtos/BookingResponses.cs

namespace SP.Application.Bookings.Dtos;

public sealed record BookingSummaryResponse(
    Guid Id,
    Guid ClientId,
    Guid ProviderId,
    Guid ServiceId,
    string ServiceName,
    string Status,
    DateTime ScheduledDate,
    DateTime CreatedAt,
    string ProviderName,
    string ProviderAvatarUrl,
    decimal ServicePrice,
    int ServiceDurationMinutes,
    bool IsReviewed);

public sealed record BookingDetailResponse(
    Guid Id,
    Guid ClientId,
    Guid ProviderId,
    Guid ServiceId,
    string ServiceName,
    string Status,
    DateTime ScheduledDate,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    ReviewResponse? Review,
    ReportResponse? Report);

public sealed record ReviewResponse(
    Guid Id,
    int Rating,
    string Comment,
    DateTime CreatedAt);

public sealed record ReportResponse(
    Guid Id,
    string Reason,
    string Status,
    DateTime CreatedAt);
