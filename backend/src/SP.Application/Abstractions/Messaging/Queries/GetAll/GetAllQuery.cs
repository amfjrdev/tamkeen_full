using SP.Domain.Abstractions;

namespace SP.Application.Abstractions.Messaging.Commands.Create;

public record GetAllQuery<TResponse> : IQuery<IReadOnlyList<TResponse>>;

public abstract class GetAllQueryHandler<TEntity, TResponse>(
    IRepository<TEntity> repository)
    : IQueryHandler<GetAllQuery<TResponse>, IReadOnlyList<TResponse>>
    where TEntity : Entity
{
    public async Task<Result<IReadOnlyList<TResponse>>> HandleAsync(
        GetAllQuery<TResponse> query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(cancellationToken);

        return Result.Success(
            entities.Select(MapToResponse).ToList() as IReadOnlyList<TResponse>);
    }

    protected abstract TResponse MapToResponse(TEntity entity);
}