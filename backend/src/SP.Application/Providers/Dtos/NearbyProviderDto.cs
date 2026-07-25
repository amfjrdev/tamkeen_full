using System;

namespace SP.Application.Providers.Dtos;

public sealed record NearbyProviderDto(
    Guid Id,
    Guid ProviderId,
    string FirstName,
    string LastName,
    string AvatarUrl,
    double Latitude,
    double Longitude,
    double DistanceKm,
    bool IsAvailable,
    decimal HourlyRate,
    double Rating,
    int ReviewCount);
