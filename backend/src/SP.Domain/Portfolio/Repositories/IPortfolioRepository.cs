using SP.Domain.Abstractions;

namespace SP.Domain.Portfolio.Repositories;

public interface IPortfolioRepository : IRepository<Portfolio>
{
    Task<Portfolio?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
}