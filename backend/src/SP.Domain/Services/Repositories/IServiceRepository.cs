using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SP.Domain.Abstractions;

namespace SP.Domain.Services.Repositories;

public interface IServiceRepository : IRepository<Service>
{
    Task<IEnumerable<Service>> GetActiveServicesByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetAllActiveServicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> SearchServicesAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> ExistsForProviderAsync(Guid providerId, string serviceName, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<(Service Service, string ProviderName, string ProviderEmail, double ProviderRating, int ProviderReviewCount, double? Latitude, double? Longitude)> Items, int TotalCount)> GetServicesWithDetailsAsync(
        Guid? categoryId,
        string? searchTerm,
        Guid? providerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}