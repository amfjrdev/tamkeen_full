using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Providers.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.ProviderProfiles.Repositories;

namespace SP.Application.Providers.Queries.SearchProviders;

public sealed record SearchProvidersQuery(
    Guid? CategoryId,
    string? Search,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    double? MinRating = null,
    bool? AvailableOnly = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    double? DistanceMaxKm = null,
    double? Lat = null,
    double? Lng = null) : IQuery<ProviderSearchResponse>;

public sealed class SearchProvidersQueryHandler : IQueryHandler<SearchProvidersQuery, ProviderSearchResponse>
{
    private readonly IProviderProfileRepository _profileRepository;

    public SearchProvidersQueryHandler(IProviderProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<Result<ProviderSearchResponse>> HandleAsync(
        SearchProvidersQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Page <= 0)
            return Result.Failure<ProviderSearchResponse>(new Error("SearchProviders.InvalidPage", "Page must be greater than zero."));

        if (query.PageSize <= 0)
            return Result.Failure<ProviderSearchResponse>(new Error("SearchProviders.InvalidPageSize", "Page size must be greater than zero."));

        if (query.MinRating.HasValue && (query.MinRating.Value < 0 || query.MinRating.Value > 5))
            return Result.Failure<ProviderSearchResponse>(new Error("SearchProviders.InvalidMinRating", "Min rating must be between 0 and 5."));

        if (query.DistanceMaxKm.HasValue && query.DistanceMaxKm.Value < 0)
            return Result.Failure<ProviderSearchResponse>(new Error("SearchProviders.InvalidDistanceMax", "Distance max cannot be negative."));

        var (items, totalCount) = await _profileRepository.SearchProvidersAsync(
            query.CategoryId,
            query.Search,
            query.Page,
            query.PageSize,
            query.SortBy,
            query.MinRating,
            query.AvailableOnly,
            query.PriceMin,
            query.PriceMax,
            query.DistanceMaxKm,
            query.Lat,
            query.Lng,
            cancellationToken);

        var responseItems = items
            .Select(x => new ProviderSearchResultDto(
                x.Id,
                x.ProviderId,
                x.FirstName,
                x.LastName,
                x.AvatarUrl,
                x.Rating,
                x.ReviewCount,
                x.DistanceKm,
                x.IsAvailable,
                x.HourlyRate))
            .ToList();

        return Result.Success(new ProviderSearchResponse(responseItems, totalCount));
    }
}
