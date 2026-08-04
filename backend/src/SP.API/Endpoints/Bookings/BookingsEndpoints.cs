using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Bookings.Commands.AcceptBooking;
using SP.Application.Bookings.Commands.AddReport;
using SP.Application.Bookings.Commands.AddReview;
using SP.Application.Bookings.Commands.CancelBooking;
using SP.Application.Bookings.Commands.CompleteBooking;
using SP.Application.Bookings.Commands.CreateBooking;
using SP.Application.Bookings.Commands.RejectBooking;
using SP.Application.Bookings.Queries.GetBookingById;
using SP.Application.Bookings.Queries.GetBookings;
using SP.Domain.Bookings;

namespace SP.API.Endpoints.Bookings;

public static class BookingsEndpoints
{
    public static RouteGroupBuilder MapBookingsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", Create).RequireAuthorization(AuthorizationPolicies.ClientOnly);
        group.MapPost("/{id:guid}/accept", Accept).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapPost("/{id:guid}/reject", Reject).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapPost("/{id:guid}/cancel", Cancel).RequireAuthorization();
        group.MapPost("/{id:guid}/complete", Complete).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapPost("/{id:guid}/review", Review).RequireAuthorization(AuthorizationPolicies.ClientOnly);
        group.MapPost("/{id:guid}/report", Report).RequireAuthorization();
        group.MapGet("/{id:guid}", GetById).RequireAuthorization();
        group.MapGet("/", GetAll).RequireAuthorization();

        return group;
    }

    private static async Task<IResult> Create(
        [FromBody] CreateBookingBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new CreateBookingCommand(currentUser.UserId, body.ServiceId, body.ScheduledDate), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Created($"/api/bookings/{result.Value}", new { id = result.Value });
    }

    private static async Task<IResult> Accept(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new AcceptBookingCommand(id, currentUser.UserId), ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> Reject(
        Guid id,
        [FromBody] RejectBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new RejectBookingCommand(id, currentUser.UserId), ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> Cancel(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new CancelBookingCommand(id, currentUser.UserId), ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> Complete(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new CompleteBookingCommand(id, currentUser.UserId), ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> Review(
        Guid id,
        [FromBody] ReviewBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new AddReviewCommand(id, currentUser.UserId, body.Rating, body.Comment), ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> Report(
        Guid id,
        [FromBody] ReportBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new AddReportCommand(id, currentUser.UserId, body.Reason), ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> GetById(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetBookingByIdQuery(id, currentUser.UserId), ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetAll(
        [AsParameters] GetBookingsParams p,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        BookingStatus? status = p.Status is not null && Enum.TryParse<BookingStatus>(p.Status, true, out var s)
            ? s : null;

        var result = await dispatcher.QueryAsync(
            new GetBookingsQuery(currentUser.UserId, status, p.StartDate, p.EndDate, p.Page, p.PageSize), ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.Ok(result.Value);
    }

    private sealed record CreateBookingBody(Guid ServiceId, DateTime ScheduledDate);
    private sealed record RejectBody(string? Reason);
    private sealed record ReviewBody(int Rating, string Comment);
    private sealed record ReportBody(string Reason);
    private sealed record GetBookingsParams(
        string? Status, DateTime? StartDate, DateTime? EndDate, int Page = 1, int PageSize = 10);
}
