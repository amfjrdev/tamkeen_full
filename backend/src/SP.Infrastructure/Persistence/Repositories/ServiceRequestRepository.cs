using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SP.Domain.ServiceRequests;
using SP.Domain.ServiceRequests.Enums;
using SP.Domain.ServiceRequests.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class ServiceRequestRepository : Repository<ServiceRequest>, IServiceRequestRepository
{
    public ServiceRequestRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.ServiceRequests
            .Include(r => r.Applications)
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ServiceRequest?> GetByIdWithApplicationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.ServiceRequests
            .Include(r => r.Applications)
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<ServiceRequest> Items, int TotalCount)> GetClientRequestsAsync(
        Guid clientId,
        ServiceRequestStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.ServiceRequests
            .Include(r => r.Applications)
            .Include(r => r.Review)
            .Where(r => r.ClientId == clientId);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<ServiceRequest> Items, int TotalCount)> GetAvailableRequestsForWilayaAsync(
        string wilaya,
        Guid? categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.ServiceRequests
            .Include(r => r.Applications)
            .Where(r => r.Status == ServiceRequestStatus.Approved && r.Wilaya.ToLower() == wilaya.ToLower());

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            query = query.Where(r => r.CategoryId == categoryId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<ServiceRequest> Items, int TotalCount)> GetProviderAppliedRequestsAsync(
        Guid providerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.ServiceRequests
            .Include(r => r.Applications)
            .Include(r => r.Review)
            .Where(r => r.Applications.Any(a => a.ProviderId == providerId));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<ServiceRequest> Items, int TotalCount)> GetForAdminAsync(
        ServiceRequestStatus? status,
        string? wilaya,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.ServiceRequests
            .Include(r => r.Applications)
            .Include(r => r.Review)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(wilaya))
            query = query.Where(r => r.Wilaya.ToLower() == wilaya.Trim().ToLower());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(s) || r.Description.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> HasProviderAppliedAsync(Guid requestId, Guid providerId, CancellationToken cancellationToken = default)
    {
        return await Context.ServiceRequestApplications
            .AnyAsync(a => a.ServiceRequestId == requestId && a.ProviderId == providerId, cancellationToken);
    }
}
