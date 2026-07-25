using SP.Application.Abstractions.Messaging;
using SP.Application.Reviews.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Repositories;

namespace SP.Application.Reviews.Queries.GetReviews;

public sealed record GetReviewsQuery(
    Guid ProviderId,
    int Page = 1,
    int PageSize = 10) : IQuery<ReviewsResponse>;

public sealed class GetReviewsQueryHandler
    : IQueryHandler<GetReviewsQuery, ReviewsResponse>
{
    private readonly IBookingRepository _bookingRepository;

    public GetReviewsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<ReviewsResponse>> HandleAsync(
        GetReviewsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ProviderId == Guid.Empty)
            return Result.Failure<ReviewsResponse>(new Error("Reviews.InvalidProviderId", "ProviderId cannot be empty."));

        if (query.Page <= 0)
            return Result.Failure<ReviewsResponse>(new Error("Reviews.InvalidPage", "Page must be greater than zero."));

        if (query.PageSize <= 0)
            return Result.Failure<ReviewsResponse>(new Error("Reviews.InvalidPageSize", "Page size must be greater than zero."));

        var (items, totalCount, averageRating) =
            await _bookingRepository.GetReviewsByProviderAsync(
                query.ProviderId, query.Page, query.PageSize, cancellationToken);

        var reviewItems = items
            .Select(r => new ReviewItemResponse(
                r.ReviewId,
                r.ClientName,
                r.ClientAvatarUrl,
                r.Rating,
                r.Comment,
                r.CreatedAt))
            .ToList();

        return Result.Success(new ReviewsResponse(reviewItems, totalCount, averageRating));
    }
}
