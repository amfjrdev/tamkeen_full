using SP.Domain.Abstractions;

namespace SP.Application.Abstractions.Messaging.Commands.Update;

public record UpdateCommand<T>(Guid Id, T Entity) : ICommand;

public abstract class UpdateCommandHandler<TEntity, TUpdateData>(
    IRepository<TEntity> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCommand<TUpdateData>>
    where TEntity : Entity
{
    public async Task<Result> HandleAsync(
        UpdateCommand<TUpdateData> command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(NotFoundError());

        var validationResult = await Validate(command.Entity, entity);
        if (validationResult.IsFailure)
            return validationResult;

        var updateResult = UpdateEntity(entity, command.Entity);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    protected abstract Error NotFoundError();

    // Override for cross-entity validation (e.g. duplicate name check excluding current)
    protected virtual Task<Result> Validate(TUpdateData data, TEntity entity) =>
        Task.FromResult(Result.Success());

    // Calls the domain method — returns Result to bubble up domain errors
    protected abstract Result UpdateEntity(TEntity entity, TUpdateData data);
}