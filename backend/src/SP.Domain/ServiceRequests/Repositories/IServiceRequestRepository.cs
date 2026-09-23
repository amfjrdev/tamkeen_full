using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Enums;

namespace SP.Domain.ServiceRequests.Repositories;

public interface IServiceRequestRepository : IRepository<ServiceRequest>
{
    Task<ServiceRequest?> GetByIdWithApplicationsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ServiceRequest> Items, int TotalCount)> GetClientRequestsAsync(
        Guid clientId,
        ServiceRequestStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ServiceRequest> Items, int TotalCount)> GetAvailableRequestsForWilayaAsync(
        string wilaya,
        Guid? categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ServiceRequest> Items, int TotalCount)> GetProviderAppliedRequestsAsync(
        Guid providerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ServiceRequest> Items, int TotalCount)> GetForAdminAsync(
        ServiceRequestStatus? status,
        string? wilaya,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> HasProviderAppliedAsync(Guid requestId, Guid providerId, CancellationToken cancellationToken = default);
}
