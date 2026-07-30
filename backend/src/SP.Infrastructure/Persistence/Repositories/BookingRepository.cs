// SP.Infrastructure/Persistence/Repositories/BookingRepository.cs

using Microsoft.EntityFrameworkCore;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Booking>> GetByClientIdAsync(
        Guid clientId, CancellationToken cancellationToken = default)
        => await Context.Bookings
            .Where(b => b.ClientId == clientId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Booking>> GetByProviderIdAsync(
        Guid providerId, CancellationToken cancellationToken = default)
        => await Context.Bookings
            .Where(b => b.ProviderId == providerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Booking>> GetByServiceIdAsync(
        Guid serviceId, CancellationToken cancellationToken = default)
        => await Context.Bookings
            .Where(b => b.ServiceId == serviceId)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasActiveBookingAsync(
        Guid clientId, Guid serviceId, CancellationToken cancellationToken = default)
        => await Context.Bookings.AnyAsync(b =>
            b.ClientId == clientId &&
            b.ServiceId == serviceId &&
            b.Status != BookingStatus.Cancelled &&
            b.Status != BookingStatus.Completed &&
            b.Status != BookingStatus.Rejected &&
            b.Status != BookingStatus.Expired,
            cancellationToken);

    public async Task<(IEnumerable<Booking> Bookings, int TotalCount)> GetBookingHistoryAsync(
        Guid userId,
        BookingStatus? status,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Bookings
            .Where(b => b.ClientId == userId || b.ProviderId == userId);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(b => b.ScheduledDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(b => b.ScheduledDate <= endDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (bookings, totalCount);
    }

    public async Task<IEnumerable<Booking>> GetPendingBookingsAsync(
        CancellationToken cancellationToken = default)
        => await Context.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .ToListAsync(cancellationToken);

    public async Task<(int JobsDone, int ReviewCount, double AverageRating)> GetProviderStatsAsync(
        Guid providerId, CancellationToken cancellationToken = default)
    {
        var jobsDone = await Context.Bookings
            .CountAsync(b => b.ProviderId == providerId && b.Status == BookingStatus.Completed,
                cancellationToken);

        var reviewStats = await Context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId && b.Review != null)
            .GroupBy(b => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(b => (double)b.Review!.Rating)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var reviewCount = reviewStats?.Count ?? 0;
        var averageRating = reviewStats?.Average ?? 0.0;

        return (jobsDone, reviewCount, Math.Round(averageRating, 1));
    }

    public async Task<(IReadOnlyList<(Guid ReviewId, Guid ClientId, string ClientName, string ClientAvatarUrl, int Rating, string Comment, DateTime CreatedAt)> Items, int TotalCount, double AverageRating)>
        GetReviewsByProviderAsync(
            Guid providerId, int page, int pageSize,
            CancellationToken cancellationToken = default)
    {
        var query = from b in Context.Bookings
                    join u in Context.Users on b.ClientId equals u.Id
                    where b.ProviderId == providerId && b.Review != null
                    select new
                    {
                        ReviewId = b.Review!.Id,
                        ClientId = b.ClientId,
                        ClientName = u.FirstName + " " + u.LastName,
                        ClientAvatarUrl = u.UserProfilePicture.Url,
                        Rating = b.Review.Rating,
                        Comment = b.Review.Comment,
                        CreatedAt = b.Review.CreatedAt
                    };

        var totalCount = await query.CountAsync(cancellationToken);

        var pagedItems = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var averageRating = totalCount > 0
            ? await Context.Bookings
                .Where(b => b.ProviderId == providerId && b.Review != null)
                .AverageAsync(b => (double)b.Review!.Rating, cancellationToken)
            : 0.0;

        var itemsTuple = pagedItems
            .Select(r => (r.ReviewId, r.ClientId, r.ClientName, r.ClientAvatarUrl, r.Rating, r.Comment, r.CreatedAt))
            .ToList();

        return (itemsTuple, totalCount, Math.Round(averageRating, 1));
    }

    public async Task<(IReadOnlyList<(Guid Id, Guid BookingId, Guid ReporterId, string Reason, string Status, DateTime CreatedAt)> Items, int TotalCount)> GetReportsPaginatedAsync(
        ReportStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Bookings
            .AsNoTracking()
            .Where(b => b.Report != null);

        if (status.HasValue)
        {
            query = query.Where(b => b.Report!.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var itemsData = await query
            .OrderByDescending(b => b.Report!.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                b.Report!.Id,
                BookingId = b.Id,
                b.Report.ReporterId,
                b.Report.Reason,
                Status = b.Report.Status.ToString(),
                b.Report.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var items = itemsData.Select(i => (
            i.Id,
            i.BookingId,
            i.ReporterId,
            i.Reason,
            i.Status,
            i.CreatedAt
        )).ToList();

        return (items, totalCount);
    }
}
