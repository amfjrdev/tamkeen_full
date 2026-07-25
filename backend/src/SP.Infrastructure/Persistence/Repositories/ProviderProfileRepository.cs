using Microsoft.EntityFrameworkCore;
using SP.Domain.ProviderProfiles;
using SP.Domain.ProviderProfiles.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class ProviderProfileRepository
    : Repository<ProviderProfile>, IProviderProfileRepository
{
    public ProviderProfileRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ProviderProfile?> GetByProviderIdAsync(
        Guid providerId, CancellationToken cancellationToken = default)
        => await Context.ProviderProfiles
            .FirstOrDefaultAsync(p => p.ProviderId == providerId, cancellationToken);

    public async Task<bool> ExistsByProviderIdAsync(
        Guid providerId, CancellationToken cancellationToken = default)
        => await Context.ProviderProfiles
            .AnyAsync(p => p.ProviderId == providerId, cancellationToken);

    public async Task<(IReadOnlyList<(Guid Id, Guid ProviderId, string FirstName, string LastName, string AvatarUrl, double Rating, int ReviewCount, double? DistanceKm, bool IsAvailable, decimal HourlyRate)> Items, int TotalCount)> SearchProvidersAsync(
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
        CancellationToken cancellationToken = default)
    {
        var bookingsStats = from b in Context.Bookings
                            where b.Review != null
                            group b by b.ProviderId into g
                            select new
                            {
                                ProviderId = g.Key,
                                AverageRating = (double?)g.Average(b => (double)b.Review!.Rating),
                                ReviewCount = (int?)g.Count()
                            };

        var query = from p in Context.ProviderProfiles
                    join u in Context.Users on p.ProviderId equals u.Id
                    join stats in bookingsStats on p.ProviderId equals stats.ProviderId into statsJoined
                    from s in statsJoined.DefaultIfEmpty()
                    select new
                    {
                        p.Id,
                        p.ProviderId,
                        u.FirstName,
                        u.LastName,
                        AvatarUrl = u.UserProfilePicture.Url,
                        Rating = s.AverageRating ?? 0.0,
                        ReviewCount = s.ReviewCount ?? 0,
                        p.IsAvailable,
                        p.HourlyRate,
                        p.Latitude,
                        p.Longitude
                    };

        if (categoryId.HasValue)
        {
            query = query.Where(x => Context.Services.Any(s => s.ProviderId == x.ProviderId && s.CategoryId == categoryId.Value && s.IsActive));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim();
            query = query.Where(x =>
                x.FirstName.Contains(cleanSearch) ||
                x.LastName.Contains(cleanSearch) ||
                Context.Services.Any(s => s.ProviderId == x.ProviderId && s.IsActive && (s.Name.Contains(cleanSearch) || s.Description.Contains(cleanSearch)))
            );
        }

        if (availableOnly == true)
        {
            query = query.Where(x => x.IsAvailable);
        }

        if (minRating.HasValue)
        {
            query = query.Where(x => x.Rating >= minRating.Value);
        }

        if (priceMin.HasValue)
        {
            query = query.Where(x => x.HourlyRate >= priceMin.Value);
        }

        if (priceMax.HasValue)
        {
            query = query.Where(x => x.HourlyRate <= priceMax.Value);
        }

        // Bounding box optimization for distance filter
        if (distanceMaxKm.HasValue && lat.HasValue && lng.HasValue)
        {
            double latRange = distanceMaxKm.Value / 111.0;
            double lngRange = distanceMaxKm.Value / (111.0 * Math.Cos(lat.Value * Math.PI / 180.0));
            double minLat = lat.Value - latRange;
            double maxLat = lat.Value + latRange;
            double minLng = lng.Value - Math.Abs(lngRange);
            double maxLng = lng.Value + Math.Abs(lngRange);

            query = query.Where(x => x.Latitude.HasValue && x.Latitude.Value >= minLat && x.Latitude.Value <= maxLat &&
                                     x.Longitude.HasValue && x.Longitude.Value >= minLng && x.Longitude.Value <= maxLng);
        }

        var candidates = await query.ToListAsync(cancellationToken);

        var results = candidates.Select(x =>
        {
            double? distanceKm = null;
            if (lat.HasValue && lng.HasValue && x.Latitude.HasValue && x.Longitude.HasValue)
            {
                distanceKm = CalculateHaversineKm(lat.Value, lng.Value, x.Latitude.Value, x.Longitude.Value);
            }

            return new
            {
                x.Id,
                x.ProviderId,
                x.FirstName,
                x.LastName,
                x.AvatarUrl,
                Rating = Math.Round(x.Rating, 1),
                x.ReviewCount,
                DistanceKm = distanceKm,
                Availability = x.IsAvailable,
                x.HourlyRate
            };
        }).ToList();

        if (distanceMaxKm.HasValue && lat.HasValue && lng.HasValue)
        {
            results = results.Where(r => r.DistanceKm.HasValue && r.DistanceKm.Value <= distanceMaxKm.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            results = sortBy.ToLower() switch
            {
                "nearest" when lat.HasValue && lng.HasValue => results.OrderBy(r => r.DistanceKm ?? double.MaxValue).ToList(),
                "rating" => results.OrderByDescending(r => r.Rating).ToList(),
                "price" => results.OrderBy(r => r.HourlyRate).ToList(),
                _ => results.OrderByDescending(r => r.Rating).ToList()
            };
        }
        else
        {
            results = results.OrderByDescending(r => r.Rating).ToList();
        }

        var totalCount = results.Count;

        var pagedResults = results
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => (
                r.Id,
                r.ProviderId,
                r.FirstName,
                r.LastName,
                r.AvatarUrl,
                r.Rating,
                r.ReviewCount,
                r.DistanceKm,
                r.Availability,
                r.HourlyRate
            ))
            .ToList();

        return (pagedResults, totalCount);
    }

    public async Task<IReadOnlyList<(Guid Id, Guid ProviderId, string FirstName, string LastName, string AvatarUrl, double Rating, int ReviewCount, double Latitude, double Longitude, double DistanceKm, bool IsAvailable, decimal HourlyRate)>> GetNearbyProvidersAsync(
        double lat,
        double lng,
        double radiusKm,
        CancellationToken cancellationToken = default)
    {
        var bookingsStats = from b in Context.Bookings
                            where b.Review != null
                            group b by b.ProviderId into g
                            select new
                            {
                                ProviderId = g.Key,
                                AverageRating = (double?)g.Average(b => (double)b.Review!.Rating),
                                ReviewCount = (int?)g.Count()
                            };

        double latRange = radiusKm / 111.0;
        double lngRange = radiusKm / (111.0 * Math.Cos(lat * Math.PI / 180.0));
        double minLat = lat - latRange;
        double maxLat = lat + latRange;
        double minLng = lng - Math.Abs(lngRange);
        double maxLng = lng + Math.Abs(lngRange);

        var query = from p in Context.ProviderProfiles
                    join u in Context.Users on p.ProviderId equals u.Id
                    join stats in bookingsStats on p.ProviderId equals stats.ProviderId into statsJoined
                    from s in statsJoined.DefaultIfEmpty()
                    where p.Latitude.HasValue && p.Latitude.Value >= minLat && p.Latitude.Value <= maxLat &&
                          p.Longitude.HasValue && p.Longitude.Value >= minLng && p.Longitude.Value <= maxLng
                    select new
                    {
                        p.Id,
                        p.ProviderId,
                        u.FirstName,
                        u.LastName,
                        AvatarUrl = u.UserProfilePicture.Url,
                        Rating = s.AverageRating ?? 0.0,
                        ReviewCount = s.ReviewCount ?? 0,
                        p.IsAvailable,
                        p.HourlyRate,
                        Latitude = p.Latitude ?? 0.0,
                        Longitude = p.Longitude ?? 0.0
                    };

        var candidates = await query.ToListAsync(cancellationToken);

        var results = candidates.Select(x =>
        {
            var distance = CalculateHaversineKm(lat, lng, x.Latitude, x.Longitude);
            return new
            {
                x.Id,
                x.ProviderId,
                x.FirstName,
                x.LastName,
                x.AvatarUrl,
                Rating = Math.Round(x.Rating, 1),
                x.ReviewCount,
                x.Latitude,
                x.Longitude,
                DistanceKm = distance,
                x.IsAvailable,
                x.HourlyRate
            };
        })
        .Where(r => r.DistanceKm <= radiusKm)
        .OrderBy(r => r.DistanceKm)
        .Select(r => (
            r.Id,
            r.ProviderId,
            r.FirstName,
            r.LastName,
            r.AvatarUrl,
            r.Rating,
            r.ReviewCount,
            r.Latitude,
            r.Longitude,
            r.DistanceKm,
            r.IsAvailable,
            r.HourlyRate
        ))
        .ToList();

        return results;
    }

    private static double CalculateHaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
