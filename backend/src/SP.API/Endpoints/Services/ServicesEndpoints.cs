using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Services.Commands.ActivateService;
using SP.Application.Services.Commands.CreateService;
using SP.Application.Services.Commands.DeactivateService;
using SP.Application.Services.Commands.UpdateService;
using SP.Application.Services.Queries.GetServiceById;
using SP.Application.Services.Queries.GetServices;

namespace SP.API.Endpoints.Services;

public static class ServicesEndpoints
{
    public static RouteGroupBuilder MapServicesEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", Create).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapPut("/{id:guid}", Update).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapPatch("/{id:guid}/activate", Activate).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapPatch("/{id:guid}/deactivate", Deactivate).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapGet("/{id:guid}", GetById).AllowAnonymous();
        group.MapGet("/", GetAll).AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Create(
        [FromBody] CreateServiceBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new CreateServiceCommand(
                currentUser.UserId,
                body.CategoryId,
                body.Name,
                body.Description,
                body.Price,
                body.DurationMinutes), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Created($"/api/services/{result.Value}", new { id = result.Value });
    }

    private static async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateServiceBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new UpdateServiceCommand(id, currentUser.UserId, body.Name, body.Description, body.Price, body.DurationMinutes), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> Activate(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new ActivateServiceCommand(id, currentUser.UserId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> Deactivate(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new DeactivateServiceCommand(id, currentUser.UserId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetServiceByIdQuery(id), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetAll(
        [AsParameters] GetServicesParams p,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(
            new GetServicesQuery(p.CategoryId, p.Search, p.ProviderId, p.Lat, p.Lng, p.Page, p.PageSize), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private sealed record CreateServiceBody(
        Guid CategoryId, string Name, string Description, decimal Price, int DurationMinutes);

    private sealed record UpdateServiceBody(
        string Name, string Description, decimal Price, int DurationMinutes);

    private sealed record GetServicesParams(
        Guid? CategoryId, string? Search, Guid? ProviderId, double? Lat = null, double? Lng = null, int Page = 1, int PageSize = 10);
}
