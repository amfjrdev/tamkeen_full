namespace SP.Application.Providers.Queries.GetNearbyProviders;

public sealed record NearbyProviderResponse(
    Guid Id,
    Guid ProviderId,
    string Name,
    string Specialty,
    double Rating,
    int ReviewCount,
    double DistanceKm,
    decimal StartingPrice,
    string? AvatarUrl,
    double Latitude,
    double Longitude,
    bool IsAvailable
);
