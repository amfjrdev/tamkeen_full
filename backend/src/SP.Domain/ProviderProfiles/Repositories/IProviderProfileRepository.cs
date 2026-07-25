using SP.Domain.Abstractions;

namespace SP.Domain.ProviderProfiles.Repositories;

public interface IProviderProfileRepository : IRepository<ProviderProfile>
{
    Task<ProviderProfile?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<(Guid Id, Guid ProviderId, string FirstName, string LastName, string AvatarUrl, double Rating, int ReviewCount, double? DistanceKm, bool IsAvailable, decimal HourlyRate)> Items, int TotalCount)> SearchProvidersAsync(
        Guid? categoryId,
        string? search,
        int page,
        int pageSize,
        string? sortBy,
        double? minRating,
        bool? availableOnly,
        decimal? priceMin,
        decimal? priceMax,
        double? distanceMaxKm,
        double? lat,
        double? lng,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(Guid Id, Guid ProviderId, string FirstName, string LastName, string AvatarUrl, double Rating, int ReviewCount, double Latitude, double Longitude, double DistanceKm, bool IsAvailable, decimal HourlyRate)>> GetNearbyProvidersAsync(
        double lat,
        double lng,
        double radiusKm,
        CancellationToken cancellationToken = default);
}
