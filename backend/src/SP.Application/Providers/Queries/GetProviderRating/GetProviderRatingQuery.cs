using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Repositories;

namespace SP.Application.Providers.Queries.GetProviderRating;

public sealed record ProviderRatingResponse(double Rating, int ReviewCount);

public sealed record GetProviderRatingQuery(Guid ProviderId) : IQuery<ProviderRatingResponse>;

public sealed class GetProviderRatingQueryHandler : IQueryHandler<GetProviderRatingQuery, ProviderRatingResponse>
{
    private readonly IBookingRepository _bookingRepository;

    public GetProviderRatingQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<ProviderRatingResponse>> HandleAsync(
        GetProviderRatingQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ProviderId == Guid.Empty)
        {
            return Result.Failure<ProviderRatingResponse>(new Error("ProviderRating.InvalidProviderId", "ProviderId cannot be empty."));
        }

        var (_, reviewCount, averageRating) = await _bookingRepository.GetProviderStatsAsync(query.ProviderId, cancellationToken);
        return Result.Success(new ProviderRatingResponse(averageRating, reviewCount));
    }
}
