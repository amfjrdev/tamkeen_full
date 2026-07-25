// SP.Application/Portfolio/Queries/GetPortfolio/GetPortfolioQuery.cs

using SP.Application.Abstractions.Messaging;
using SP.Application.Portfolio.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Portfolio.Errors;
using SP.Domain.Portfolio.Repositories;

namespace SP.Application.Portfolio.Queries.GetPortfolio;

public sealed record GetPortfolioQuery(Guid ProviderId) : IQuery<PortfolioResponse>;

public sealed class GetPortfolioQueryHandler : IQueryHandler<GetPortfolioQuery, PortfolioResponse>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPortfolioQueryHandler(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Result<PortfolioResponse>> HandleAsync(
        GetPortfolioQuery query,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);
        if (portfolio is null)
            return Result.Failure<PortfolioResponse>(PortfolioErrors.NotFound);

        var projects = portfolio.Projects.Select(p => new ProjectResponse(
            p.Id,
            p.Name,
            p.Description,
            p.ProjectImages.Select(img => img.Url).ToList())).ToList();

        return Result.Success(new PortfolioResponse(portfolio.Id, portfolio.ProviderId, projects));
    }
}
