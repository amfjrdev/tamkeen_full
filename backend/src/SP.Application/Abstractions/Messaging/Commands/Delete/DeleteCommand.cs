using SP.Domain.Abstractions;

namespace SP.Application.Abstractions.Messaging.Commands.Delete;

public record DeleteCommand(Guid Id) : ICommand;

public abstract class DeleteCommandHandler<TEntity>(
    IRepository<TEntity> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCommand>
    where TEntity : Entity
{
    public async Task<Result> HandleAsync(
        DeleteCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(NotFoundError());

        var validationResult = await Validate(entity);
        if (validationResult.IsFailure)
            return validationResult;

        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    // Override to provide the entity-specific not-found error
    protected abstract Error NotFoundError();

    // Override to add pre-delete validation (e.g. can't delete if has active bookings)
    protected virtual Task<Result> Validate(TEntity entity) =>
        Task.FromResult(Result.Success());

    
}