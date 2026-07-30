// SP.Infrastructure/Persistence/Repositories/ServiceRepository.cs

using Microsoft.EntityFrameworkCore;
using SP.Domain.Services;
using SP.Domain.Services.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Service>> GetActiveServicesByProviderIdAsync(
        Guid providerId, CancellationToken cancellationToken = default)
        => await Context.Services
            .AsNoTracking()
            .Where(s => s.ProviderId == providerId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Service>> GetActiveServicesByProviderIdsAsync(
        IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default)
        => await Context.Services
            .AsNoTracking()
            .Where(s => providerIds.Contains(s.ProviderId) && s.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Service>> GetAllActiveServicesAsync(
        CancellationToken cancellationToken = default)
        => await Context.Services
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Service>> SearchServicesAsync(
        string searchTerm, CancellationToken cancellationToken = default)
    {
        // EF.Functions.Contains translates to a parameterized LIKE — no injection risk
        var term = searchTerm.Trim();
        return await Context.Services
            .AsNoTracking()
            .Where(s => s.IsActive &&
                (s.Name.Contains(term) || s.Description.Contains(term)))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForProviderAsync(
        Guid providerId, string serviceName, CancellationToken cancellationToken = default)
        => await Context.Services
            .AnyAsync(s => s.ProviderId == providerId &&
                           s.Name.ToLower() == serviceName.Trim().ToLower(),
                      cancellationToken);

    public async Task<(IReadOnlyList<(Service Service, string ProviderName, string ProviderEmail, double ProviderRating, int ProviderReviewCount, double? Latitude, double? Longitude)> Items, int TotalCount)> GetServicesWithDetailsAsync(
        Guid? categoryId,
        string? searchTerm,
        Guid? providerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var bookingsStats = from b in Context.Bookings
                            where b.Review != null
                            group b by b.ProviderId into g
                            select new
                            {
                                ProviderId = g.Key,
                                AverageRating = (double?)g.Average(b => (double)b.Review!.Rating),
                                ReviewCount = (int?)g.Count()
                            };

        var query = from s in Context.Services
                    join u in Context.Users on s.ProviderId equals u.Id
                    join p in Context.ProviderProfiles on s.ProviderId equals p.ProviderId into pJoined
                    from prof in pJoined.DefaultIfEmpty()
                    join stats in bookingsStats on s.ProviderId equals stats.ProviderId into statsJoined
                    from st in statsJoined.DefaultIfEmpty()
                    where s.IsActive
                    select new
                    {
                        Service = s,
                        ProviderName = u.FirstName + " " + u.LastName,
                        ProviderEmail = u.Email,
                        ProviderRating = st.AverageRating ?? 0.0,
                        ProviderReviewCount = st.ReviewCount ?? 0,
                        Latitude = prof != null ? prof.Latitude : null,
                        Longitude = prof != null ? prof.Longitude : null
                    };

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.Service.CategoryId == categoryId.Value);
        }

        if (providerId.HasValue)
        {
            query = query.Where(x => x.Service.ProviderId == providerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(x => x.Service.Name.ToLower().Contains(term) || x.Service.Description.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pagedItems = await query
            .AsNoTracking()
            .OrderBy(x => x.Service.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var itemsTuple = pagedItems
            .Select(x => (x.Service, x.ProviderName, x.ProviderEmail, x.ProviderRating, x.ProviderReviewCount, x.Latitude, x.Longitude))
            .ToList();

        return (itemsTuple, totalCount);
    }

    public async Task<IReadOnlyList<Service>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await Context.Services
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
}
