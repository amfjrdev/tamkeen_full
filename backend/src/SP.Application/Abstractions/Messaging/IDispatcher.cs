using SP.Domain.Abstractions;

namespace SP.Application.Abstractions.Messaging;

public interface IDispatcher
{
    Task<Result<TResponse>> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken ct = default);

    Task<Result> SendAsync(
        ICommand command,
        CancellationToken ct = default);

    Task<Result<TResponse>> QueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken ct = default);
}