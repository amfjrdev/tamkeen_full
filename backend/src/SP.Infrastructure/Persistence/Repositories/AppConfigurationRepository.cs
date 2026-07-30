using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SP.Domain.Shared;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class AppConfigurationRepository : Repository<AppConfiguration>, IAppConfigurationRepository
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "PublicAppConfigCacheKey";

    public AppConfigurationRepository(ApplicationDbContext dbContext, IMemoryCache cache) : base(dbContext)
    {
        _cache = cache;
    }

    public async Task<List<AppConfiguration>> GetPublicConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(CacheKey, out List<AppConfiguration>? configs) || configs == null)
        {
            configs = await Context.Set<AppConfiguration>()
                .Where(x => x.IsPublic)
                .ToListAsync(cancellationToken);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(CacheKey, configs, cacheEntryOptions);
        }

        return configs;
    }

    public async Task<AppConfiguration?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        => await Context.Set<AppConfiguration>()
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

    public async Task<List<AppConfiguration>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        => await Context.Set<AppConfiguration>()
            .Where(x => x.Category == category)
            .ToListAsync(cancellationToken);

    public new async Task AddAsync(AppConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await Context.Set<AppConfiguration>().AddAsync(configuration, cancellationToken);
        _cache.Remove(CacheKey);
    }

    public Task UpdateAsync(AppConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Context.Set<AppConfiguration>().Update(configuration);
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}
