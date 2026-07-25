namespace SP.Domain.Abstractions;

// Command repository for <<<write>>> operations (EF Core)
public interface IRepository<T> where T : Entity
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

}