namespace SP.Domain.Shared;

public interface IAppConfigurationRepository
{
    Task<List<AppConfiguration>> GetPublicConfigurationsAsync(CancellationToken cancellationToken = default);
    Task<AppConfiguration?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<AppConfiguration>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task AddAsync(AppConfiguration configuration, CancellationToken cancellationToken = default);
    Task UpdateAsync(AppConfiguration configuration, CancellationToken cancellationToken = default);
}
