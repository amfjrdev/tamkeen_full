using Microsoft.Extensions.DependencyInjection;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using FluentValidation;

namespace SP.Infrastructure.Messaging;

public sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    public async Task<Result<TResponse>> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken ct = default)
    {
        // Execute validation rules if a validator is registered
        var validatorType = typeof(IValidator<>).MakeGenericType(command.GetType());
        var validator = provider.GetService(validatorType) as IValidator;
        if (validator != null)
        {
            var context = new ValidationContext<object>(command);
            var validationResult = await validator.ValidateAsync(context, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.First();
                return Result.Failure<TResponse>(new Error(
                    firstError.ErrorCode ?? "Validation.Error",
                    firstError.ErrorMessage));
            }
        }

        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(TResponse));

        dynamic handler = provider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)command, ct);
    }

    public async Task<Result> SendAsync(
        ICommand command,
        CancellationToken ct = default)
    {
        // Execute validation rules if a validator is registered
        var validatorType = typeof(IValidator<>).MakeGenericType(command.GetType());
        var validator = provider.GetService(validatorType) as IValidator;
        if (validator != null)
        {
            var context = new ValidationContext<object>(command);
            var validationResult = await validator.ValidateAsync(context, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.First();
                return Result.Failure(new Error(
                    firstError.ErrorCode ?? "Validation.Error",
                    firstError.ErrorMessage));
            }
        }

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