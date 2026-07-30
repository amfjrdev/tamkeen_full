using SP.Domain.Abstractions;
using SP.Domain.Bookings;

namespace SP.Domain.Bookings.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveBookingAsync(Guid clientId, Guid serviceId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Booking> Bookings, int TotalCount)> GetBookingHistoryAsync(
        Guid userId, 
        BookingStatus? status, 
        DateTime? startDate, 
        DateTime? endDate, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetPendingBookingsAsync(CancellationToken cancellationToken = default);

    Task<(int JobsDone, int ReviewCount, double AverageRating)> GetProviderStatsAsync(
        Guid providerId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<(Guid ReviewId, Guid ClientId, string ClientName, string ClientAvatarUrl, int Rating, string Comment, DateTime CreatedAt)> Items, int TotalCount, double AverageRating)>
        GetReviewsByProviderAsync(
            Guid providerId, int page, int pageSize,
            CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<(Guid Id, Guid BookingId, Guid ReporterId, string Reason, string Status, DateTime CreatedAt)> Items, int TotalCount)> GetReportsPaginatedAsync(
        ReportStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}