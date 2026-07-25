// SP.Application/Portfolio/Dtos/PortfolioResponses.cs

namespace SP.Application.Portfolio.Dtos;

public sealed record PortfolioResponse(
    Guid Id,
    Guid ProviderId,
    IReadOnlyList<ProjectResponse> Projects);

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<string> ImageUrls);

// Flat public-facing portfolio item (Feature 2)
public sealed record PortfolioItemResponse(
    Guid Id,
    string? ImageUrl,
    string Title);
