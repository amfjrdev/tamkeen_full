using SP.Application.Abstractions.Messaging;
using SP.Application.Providers.Dtos;

namespace SP.Application.Providers.Queries.GetNearbyProviders;

public sealed record GetNearbyProvidersQuery(
    double Latitude,
    double Longitude,
    double RadiusKm,
    string? CategoryId
) : IQuery<IReadOnlyList<NearbyProviderDto>>;
