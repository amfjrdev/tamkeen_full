using SP.Application.Abstractions.Messaging;
using SP.Application.Portfolio.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Portfolio.Errors;
using SP.Domain.Portfolio.Repositories;

namespace SP.Application.Portfolio.Queries.GetPortfolioItems;

public sealed record GetPortfolioItemsQuery(Guid ProviderId)
    : IQuery<IReadOnlyList<PortfolioItemResponse>>;

public sealed class GetPortfolioItemsQueryHandler
    : IQueryHandler<GetPortfolioItemsQuery, IReadOnlyList<PortfolioItemResponse>>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPortfolioItemsQueryHandler(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Result<IReadOnlyList<PortfolioItemResponse>>> HandleAsync(
        GetPortfolioItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByProviderIdAsync(
            query.ProviderId, cancellationToken);

        if (portfolio is null)
            return Result.Failure<IReadOnlyList<PortfolioItemResponse>>(PortfolioErrors.NotFound);

        var items = portfolio.Projects
            .Select(p => new PortfolioItemResponse(
                p.Id,
                p.ProjectImages.FirstOrDefault()?.Url,
                p.Name))
            .ToList();

        return Result.Success<IReadOnlyList<PortfolioItemResponse>>(items);
    }
}
