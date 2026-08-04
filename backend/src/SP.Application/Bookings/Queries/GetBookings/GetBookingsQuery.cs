// SP.Application/Bookings/Queries/GetBookings/GetBookingsQuery.cs

using SP.Application.Abstractions.Messaging;
using SP.Application.Bookings.Dtos;
using SP.Application.Common;
using SP.Domain.Abstractions;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Repositories;

using SP.Domain.Services.Repositories;

using SP.Domain.Users;

namespace SP.Application.Bookings.Queries.GetBookings;

public sealed record GetBookingsQuery(
    Guid UserId,
    BookingStatus? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedList<BookingSummaryResponse>>;

public sealed class GetBookingsQueryHandler : IQueryHandler<GetBookingsQuery, PagedList<BookingSummaryResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUserRepository _userRepository;

    public GetBookingsQueryHandler(
        IBookingRepository bookingRepository, 
        IServiceRepository serviceRepository,
        IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<PagedList<BookingSummaryResponse>>> HandleAsync(
        GetBookingsQuery query,
        CancellationToken cancellationToken = default)
    {
        var (bookings, totalCount) = await _bookingRepository.GetBookingHistoryAsync(
            query.UserId,
            query.Status,
            query.StartDate,
            query.EndDate,
            query.Page,
            query.PageSize,
            cancellationToken);

        var serviceIds = bookings.Select(b => b.ServiceId).Distinct().ToList();
        var servicesList = await _serviceRepository.GetByIdsAsync(serviceIds, cancellationToken);
        var servicesDict = servicesList.ToDictionary(s => s.Id);

        var providerIds = bookings.Select(b => b.ProviderId).Distinct().ToList();
        var providersList = await _userRepository.GetByIdsAsync(providerIds, cancellationToken);
        var providersDict = providersList.ToDictionary(u => u.Id);

        var items = bookings.Select(b =>
        {
            var serviceName = "Unknown Service";
            decimal price = 0;
            int duration = 0;
            if (servicesDict.TryGetValue(b.ServiceId, out var s))
            {
                serviceName = s.Name;
                price = s.Price;
                duration = s.DurationMinutes;
            }

            var providerName = "Unknown Provider";
            var providerAvatarUrl = "https://ui-avatars.com/api/?background=random";
            if (providersDict.TryGetValue(b.ProviderId, out var p))
            {
                providerName = p.FirstName + " " + p.LastName;
                providerAvatarUrl = p.UserProfilePicture.Url;
            }

            return new BookingSummaryResponse(
                b.Id,
                b.ClientId,
                b.ProviderId,
                b.ServiceId,
                serviceName,
                b.Status.ToString(),
                b.ScheduledDate,
                b.CreatedAt,
                providerName,
                providerAvatarUrl,
                price,
                duration,
                b.Review != null,
                b.Report != null);
        }).ToList();

        return Result.Success(new PagedList<BookingSummaryResponse>(items, query.Page, query.PageSize, totalCount));
    }
}
