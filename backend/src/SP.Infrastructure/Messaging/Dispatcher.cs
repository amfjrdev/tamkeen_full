using Microsoft.Extensions.DependencyInjection;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;

namespace SP.Infrastructure.Messaging;

public sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    public async Task<Result<TResponse>> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(TResponse));

        dynamic handler = provider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)command, ct);
    }

    public async Task<Result> SendAsync(
        ICommand command,
        CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<>)
            .MakeGenericType(command.GetType());

        dynamic handler = provider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)command, ct);
    }

    public async Task<Result<TResponse>> QueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>)
            .MakeGenericType(query.GetType(), typeof(TResponse));

        dynamic handler = provider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)query, ct);
    }
}