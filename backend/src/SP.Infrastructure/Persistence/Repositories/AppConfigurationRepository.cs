using Microsoft.EntityFrameworkCore;
using SP.Domain.Shared;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class AppConfigurationRepository : Repository<AppConfiguration>, IAppConfigurationRepository
{
    public AppConfigurationRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<List<AppConfiguration>> GetPublicConfigurationsAsync(CancellationToken cancellationToken = default)
        => await Context.Set<AppConfiguration>()
            .Where(x => x.IsPublic)
            .ToListAsync(cancellationToken);

    public async Task<AppConfiguration?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        => await Context.Set<AppConfiguration>()
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

    public async Task<List<AppConfiguration>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        => await Context.Set<AppConfiguration>()
            .Where(x => x.Category == category)
            .ToListAsync(cancellationToken);

    public new async Task AddAsync(AppConfiguration configuration, CancellationToken cancellationToken = default)
        => await Context.Set<AppConfiguration>().AddAsync(configuration, cancellationToken);

    public Task UpdateAsync(AppConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Context.Set<AppConfiguration>().Update(configuration);
        return Task.CompletedTask;
    }
}
