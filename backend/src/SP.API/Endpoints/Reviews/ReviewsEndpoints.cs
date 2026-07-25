using Microsoft.AspNetCore.Mvc;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Reviews.Queries.GetReviews;

namespace SP.API.Endpoints.Reviews;

public static class ReviewsEndpoints
{
    public static RouteGroupBuilder MapReviewsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetReviews).AllowAnonymous();

        return group;
    }

    private static async Task<IResult> GetReviews(
        [FromQuery] Guid providerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        IDispatcher dispatcher = null!,
        CancellationToken ct = default)
    {
        var result = await dispatcher.QueryAsync(
            new GetReviewsQuery(providerId, page, pageSize), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }
}
