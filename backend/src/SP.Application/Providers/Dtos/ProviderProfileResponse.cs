namespace SP.Application.Providers.Dtos;

public sealed record ProviderProfileResponse(
    Guid Id,
    Guid ProviderId,
    double Rating,
    int ReviewCount,
    int JobsDone,
    int ResponseTimeMins,
    bool IsAvailable,
    string? AboutMe,
    decimal HourlyRate,
    double? DistanceKm);
