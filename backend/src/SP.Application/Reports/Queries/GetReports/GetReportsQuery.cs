// SP.Application/Reports/Queries/GetReports/GetReportsQuery.cs

using SP.Application.Abstractions.Messaging;
using SP.Application.Common;
using SP.Application.Reports.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Repositories;

namespace SP.Application.Reports.Queries.GetReports;

public sealed record GetReportsQuery(
    ReportStatus? Status = null,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedList<ReportSummaryResponse>>;

public sealed class GetReportsQueryHandler : IQueryHandler<GetReportsQuery, PagedList<ReportSummaryResponse>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetReportsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<PagedList<ReportSummaryResponse>>> HandleAsync(
        GetReportsQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _bookingRepository.GetReportsPaginatedAsync(
            query.Status,
            query.Page,
            query.PageSize,
            cancellationToken);

        var mappedItems = items
            .Select(i => new ReportSummaryResponse(
                i.Id,
                i.BookingId,
                i.ReporterId,
                i.Reason,
                i.Status,
                i.CreatedAt))
            .ToList();

        return Result.Success(new PagedList<ReportSummaryResponse>(mappedItems, query.Page, query.PageSize, totalCount));
    }
}
