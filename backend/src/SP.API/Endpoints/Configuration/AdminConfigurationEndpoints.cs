using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Configuration.Commands.UpdateConfiguration;

namespace SP.API.Endpoints.Configuration;

public sealed record UpdateConfigurationRequest(string Key, string Value, string Category, bool IsPublic);

public static class AdminConfigurationEndpoints
{
    public static RouteGroupBuilder MapAdminConfigurationEndpoints(this RouteGroupBuilder group)
    {
        group.MapPut("/{key}", UpdateConfiguration).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        return group;
    }

    private static async Task<IResult> UpdateConfiguration(
        string key,
        [FromBody] UpdateConfigurationRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new UpdateConfigurationCommand(key, request.Value, request.Category, request.IsPublic), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }
}