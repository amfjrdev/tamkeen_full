using SP.Application.Abstractions.Messaging;
using SP.Application.Providers.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Categories.Repositories;
using SP.Domain.ProviderProfiles.Repositories;
using SP.Domain.Services.Repositories;

namespace SP.Application.Providers.Queries.GetNearbyProviders;

public sealed class GetNearbyProvidersQueryHandler
    : IQueryHandler<GetNearbyProvidersQuery, IReadOnlyList<NearbyProviderDto>>
{
    private readonly IProviderProfileRepository _providerRepository;
    private readonly IServiceRepository _serviceRepository;

    public GetNearbyProvidersQueryHandler(
        IProviderProfileRepository providerRepository,
        IServiceRepository serviceRepository)
    {
        _providerRepository = providerRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<IReadOnlyList<NearbyProviderDto>>> HandleAsync(
        GetNearbyProvidersQuery query,
        CancellationToken cancellationToken = default)
    {
        var providers = await _providerRepository.GetNearbyProvidersAsync(
            query.Latitude,
            query.Longitude,
            query.RadiusKm,
            cancellationToken);

        var response = new List<NearbyProviderDto>();

        foreach (var provider in providers)
        {
            var services = await _serviceRepository.GetActiveServicesByProviderIdAsync(
                provider.ProviderId,
                cancellationToken);

            var minPrice = services.Any() ? services.Min(s => s.Price) : 0;

            response.Add(new NearbyProviderDto(
                provider.Id,
                provider.ProviderId,
                provider.FirstName,
                provider.LastName,
                provider.AvatarUrl,
                provider.Latitude,
                provider.Longitude,
                provider.DistanceKm,
                provider.IsAvailable,
                minPrice,
                provider.Rating,
                provider.ReviewCount
            ));
        }

        return Result.Success<IReadOnlyList<NearbyProviderDto>>(response);
    }
}
