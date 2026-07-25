using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Commands.Create;
using SP.Application.Abstractions.Messaging.Commands.Update;
using SP.Application.Abstractions.Messaging.Queries.GetById;

namespace SP.API.Abstractions;

public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}

public abstract class GenericEndpoints<TCreateData, TUpdateData, TResponse>(string routePrefix)
    : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags(typeof(TResponse).Name)
            .WithOpenApi();

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
    }

    private static async Task<IResult> GetAllAsync(
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetAllQuery<TResponse>(), ct);

        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetByIdQuery<TResponse>(id), ct);

        return result.IsFailure
            ? Results.NotFound(result.Error)
            : Results.Ok(result.Value);
    }

    private async Task<IResult> CreateAsync(
        TCreateData data,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(BuildCreateCommand(data), ct);

        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.Created($"{routePrefix}/{result.Value}", new { id = result.Value });
    }

    private async Task<IResult> UpdateAsync(
        Guid id,
        TUpdateData data,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(BuildUpdateCommand(id, data), ct);

        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.NoContent();
    }

    private async Task<IResult> DeleteAsync(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(BuildDeleteCommand(id), ct);

        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.NoContent();
    }

    protected abstract CreateCommand<TCreateData> BuildCreateCommand(TCreateData data);
    protected abstract UpdateCommand<TUpdateData> BuildUpdateCommand(Guid id, TUpdateData data);
    protected abstract ICommand BuildDeleteCommand(Guid id);
}