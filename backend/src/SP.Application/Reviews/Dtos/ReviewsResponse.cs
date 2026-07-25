namespace SP.Application.Reviews.Dtos;

public sealed record ReviewItemResponse(
    Guid Id,
    string ClientName,
    string ClientAvatarUrl,
    int Rating,
    string Comment,
    DateTime CreatedAt);

public sealed record ReviewsResponse(
    IReadOnlyList<ReviewItemResponse> Items,
    int TotalCount,
    double AverageRating);
