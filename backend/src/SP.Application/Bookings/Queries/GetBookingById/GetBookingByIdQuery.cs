// SP.Application/Bookings/Queries/GetBookingById/GetBookingByIdQuery.cs

using SP.Application.Abstractions.Messaging;
using SP.Application.Bookings.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Repositories;

using SP.Domain.Services.Repositories;

namespace SP.Application.Bookings.Queries.GetBookingById;

public sealed record GetBookingByIdQuery(Guid BookingId, Guid RequesterId) : IQuery<BookingDetailResponse>;

public sealed class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, BookingDetailResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRepository _serviceRepository;

    public GetBookingByIdQueryHandler(IBookingRepository bookingRepository, IServiceRepository serviceRepository)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<BookingDetailResponse>> HandleAsync(
        GetBookingByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(query.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure<BookingDetailResponse>(BookingErrors.NotFound);

        if (booking.ClientId != query.RequesterId && booking.ProviderId != query.RequesterId)
            return Result.Failure<BookingDetailResponse>(BookingErrors.UnauthorizedAction);

        var review = booking.Review is null ? null : new ReviewResponse(
            booking.Review.Id,
            booking.Review.Rating,
            booking.Review.Comment,
            booking.Review.CreatedAt);

        var report = booking.Report is null ? null : new ReportResponse(
            booking.Report.Id,
            booking.Report.Reason,
            booking.Report.Status.ToString(),
            booking.Report.CreatedAt);

        var service = await _serviceRepository.GetByIdAsync(booking.ServiceId, cancellationToken);
        var serviceName = service?.Name ?? "Unknown Service";

        var response = new BookingDetailResponse(
            booking.Id,
            booking.ClientId,
            booking.ProviderId,
            booking.ServiceId,
            serviceName,
            booking.Status.ToString(),
            booking.ScheduledDate,
            booking.CreatedAt,
            booking.CompletedAt,
            review,
            report);

        return Result.Success(response);
    }
}
