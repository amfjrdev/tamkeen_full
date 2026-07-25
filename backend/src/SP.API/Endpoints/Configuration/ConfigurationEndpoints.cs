using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Configuration.Queries.GetAppConfig;

namespace SP.API.Endpoints.Configuration;

public static class ConfigurationEndpoints
{
    public static RouteGroupBuilder MapConfigurationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAppConfig).AllowAnonymous();
        return group;
    }

    private static async Task<IResult> GetAppConfig(
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetAppConfigQuery(), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }
}
