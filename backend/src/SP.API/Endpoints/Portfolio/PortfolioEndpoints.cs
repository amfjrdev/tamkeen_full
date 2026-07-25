using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Portfolio.Commands.AddProject;
using SP.Application.Portfolio.Commands.RemoveProject;
using SP.Application.Portfolio.Queries.GetPortfolio;

namespace SP.API.Endpoints.Portfolio;

public static class PortfolioEndpoints
{
    public static RouteGroupBuilder MapPortfolioEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/projects", AddProject).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapDelete("/projects/{projectId:guid}", RemoveProject).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapGet("/{providerId:guid}", GetPortfolioItems).AllowAnonymous();

        return group;
    }

    private static async Task<IResult> AddProject(
        [FromBody] AddProjectBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new AddProjectCommand(currentUser.UserId, body.Name, body.Description, body.ImageUrls), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Created($"/api/portfolio/{currentUser.UserId}", new { id = result.Value });
    }

    private static async Task<IResult> RemoveProject(
        Guid projectId,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new RemoveProjectCommand(currentUser.UserId, projectId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> GetPortfolioItems(
        Guid providerId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetPortfolioQuery(providerId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private sealed record AddProjectBody(string Name, string Description, List<string> ImageUrls);
}
