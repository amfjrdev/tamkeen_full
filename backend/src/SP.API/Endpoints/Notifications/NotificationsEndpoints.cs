using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Notifications.Commands.MarkNotificationAsRead;
using SP.Application.Notifications.Queries.GetNotifications;

namespace SP.API.Endpoints.Notifications;

public static class NotificationsEndpoints
{
    public static RouteGroupBuilder MapNotificationsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll).RequireAuthorization();
        group.MapPost("/{id:guid}/read", MarkAsRead).RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetAll(
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetNotificationsQuery(currentUser.UserId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> MarkAsRead(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new MarkNotificationAsReadCommand(id, currentUser.UserId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }
}
