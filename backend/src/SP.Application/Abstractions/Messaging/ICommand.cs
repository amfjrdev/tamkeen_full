

namespace SP.Application.Abstractions.Messaging;

public interface ICommand : IBaseCommand
{
    // marker interface for commands without return value
}

public interface ICommand<TResponse> : IBaseCommand
{
    // marker interface for commands that return a typed result
}
