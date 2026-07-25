using SP.Application.Abstractions.Messaging;
using SP.Application.Providers.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Repositories;
using SP.Domain.ProviderProfiles.Errors;
using SP.Domain.ProviderProfiles.Repositories;

namespace SP.Application.Providers.Queries.GetProviderProfile;

public sealed record GetProviderProfileQuery(
    Guid ProviderId,
    double? CallerLatitude,
    double? CallerLongitude) : IQuery<ProviderProfileResponse>;

public sealed class GetProviderProfileQueryHandler
    : IQueryHandler<GetProviderProfileQuery, ProviderProfileResponse>
{
    private readonly IProviderProfileRepository _profileRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetProviderProfileQueryHandler(
        IProviderProfileRepository profileRepository,
        IBookingRepository bookingRepository)
    {
        _profileRepository = profileRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<ProviderProfileResponse>> HandleAsync(
        GetProviderProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);
        if (profile is null)
            return Result.Failure<ProviderProfileResponse>(ProviderProfileErrors.NotFound);

        var (jobsDone, reviewCount, averageRating) =
            await _bookingRepository.GetProviderStatsAsync(query.ProviderId, cancellationToken);

        double? distanceKm = null;
        if (query.CallerLatitude.HasValue && query.CallerLongitude.HasValue
            && profile.Latitude.HasValue && profile.Longitude.HasValue)
        {
            distanceKm = CalculateHaversineKm(
                query.CallerLatitude.Value, query.CallerLongitude.Value,
                profile.Latitude.Value, profile.Longitude.Value);
        }

        return Result.Success(new ProviderProfileResponse(
            profile.Id,
            profile.ProviderId,
            averageRating,
            reviewCount,
            jobsDone,
            profile.ResponseTimeMins,
            profile.IsAvailable,
            profile.AboutMe,
            profile.HourlyRate,
            distanceKm));
    }

    private static double CalculateHaversineKm(
        double lat1, double lon1, double lat2, double lon2)
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
