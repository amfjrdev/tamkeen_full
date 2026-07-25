using System;
using System.Collections.Generic;

namespace SP.Application.Providers.Dtos;

public sealed record ProviderSearchResultDto(
    Guid Id,
    Guid ProviderId,
    string FirstName,
    string LastName,
    string AvatarUrl,
    double Rating,
    int ReviewCount,
    double? DistanceKm,
    bool Availability,
    decimal HourlyRate);

public sealed record ProviderSearchResponse(
    IReadOnlyList<ProviderSearchResultDto> Items,
    int TotalCount);
