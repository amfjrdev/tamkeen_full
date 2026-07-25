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
        var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);

        var reports = allBookings
            .Where(b => b.Report is not null)
            .Where(b => query.Status is null || b.Report!.Status == query.Status)
            .Select(b => new ReportSummaryResponse(
                b.Report!.Id,
                b.Id,
                b.Report.ReporterId,
                b.Report.Reason,
                b.Report.Status.ToString(),
                b.Report.CreatedAt))
            .ToList();

        var totalCount = reports.Count;
        var items = reports
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return Result.Success(new PagedList<ReportSummaryResponse>(items, query.Page, query.PageSize, totalCount));
    }
}
