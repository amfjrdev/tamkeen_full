using SP.Domain.Abstractions;

namespace SP.Application.Abstractions.Messaging.Commands.Create;

public record CreateCommand<T>(T Entity) : ICommand<Guid>;

public abstract class CreateCommandHandler<TEntity, TCreateData>(
    IRepository<TEntity> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCommand<TCreateData>, Guid>
    where TEntity : Entity
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCommand<TCreateData> command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await Validate(command.Entity);
        if (validationResult.IsFailure)
            return Result.Failure<Guid>(validationResult.Error);

        var entity = CreateEntity(command.Entity);

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Id);
    }

    protected virtual Task<Result> Validate(TCreateData data) =>
        Task.FromResult(Result.Success());

    protected abstract TEntity CreateEntity(TCreateData data);
}