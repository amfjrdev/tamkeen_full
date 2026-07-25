using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Reports.Commands.ResolveReport;
using SP.Application.Reports.Queries.GetReports;
using SP.Domain.Bookings;

namespace SP.API.Endpoints.Reports;

public static class ReportsEndpoints
{
    public static RouteGroupBuilder MapReportsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/resolve", Resolve).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/", GetAll).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return group;
    }

    private static async Task<IResult> Resolve(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new ResolveReportCommand(id), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> GetAll(
        [AsParameters] GetReportsParams p,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        ReportStatus? status = p.Status is not null && Enum.TryParse<ReportStatus>(p.Status, true, out var s)
            ? s : null;

        var result = await dispatcher.QueryAsync(new GetReportsQuery(status, p.Page, p.PageSize), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private sealed record GetReportsParams(string? Status, int Page = 1, int PageSize = 10);
}
