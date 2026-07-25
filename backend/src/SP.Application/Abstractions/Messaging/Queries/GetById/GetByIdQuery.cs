using SP.Domain.Abstractions;

namespace SP.Application.Abstractions.Messaging.Queries.GetById;

public record GetByIdQuery<TResponse>(Guid Id) : IQuery<TResponse?>;


public abstract class GetByIdQueryHandler<TEntity, TResponse>(
    IRepository<TEntity> repository)
    : IQueryHandler<GetByIdQuery<TResponse>, TResponse?>
    where TEntity : Entity
{
    public async Task<Result<TResponse?>> HandleAsync(
        GetByIdQuery<TResponse> query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (entity is null)
            return Result.Failure<TResponse?>(NotFoundError());

        return Result.Success<TResponse?>(MapToResponse(entity));
    }

    protected abstract Error NotFoundError();

    protected abstract TResponse MapToResponse(TEntity entity);
}