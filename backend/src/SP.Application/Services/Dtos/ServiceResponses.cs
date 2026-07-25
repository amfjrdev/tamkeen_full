// SP.Application/Services/Dtos/ServiceResponses.cs

namespace SP.Application.Services.Dtos;

public sealed record ServiceSummaryResponse(
    Guid Id,
    Guid ProviderId,
    Guid CategoryId,
    string Name,
    decimal Price,
    int DurationMinutes,
    bool IsActive,
    string ProviderName,
    string ProviderEmail,
    double ProviderRating,
    int ProviderReviewCount,
    double? DistanceKm);

public sealed record ServiceDetailResponse(
    Guid Id,
    Guid ProviderId,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    int DurationMinutes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
