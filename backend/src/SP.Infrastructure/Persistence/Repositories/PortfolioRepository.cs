// SP.Infrastructure/Persistence/Repositories/PortfolioRepository.cs

using Microsoft.EntityFrameworkCore;
using SP.Domain.Portfolio;
using SP.Domain.Portfolio.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class PortfolioRepository : Repository<Portfolio>, IPortfolioRepository
{
    public PortfolioRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Portfolio?> GetByProviderIdAsync(
        Guid providerId, CancellationToken cancellationToken = default)
        => await Context.Portfolios
            .FirstOrDefaultAsync(p => p.ProviderId == providerId, cancellationToken);

    public async Task<bool> ExistsByProviderIdAsync(
        Guid providerId, CancellationToken cancellationToken = default)
        => await Context.Portfolios
            .AnyAsync(p => p.ProviderId == providerId, cancellationToken);
}
