// SP.Infrastructure/Persistence/Repositories/Repository.cs

using Microsoft.EntityFrameworkCore;
using SP.Domain.Abstractions;

namespace SP.Infrastructure.Persistence.Repositories;

internal abstract class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly ApplicationDbContext Context;

    protected Repository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity)
        => Context.Set<T>().Update(entity);

    public void Remove(T entity)
        => Context.Set<T>().Remove(entity);

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Context.Set<T>().ToListAsync(cancellationToken);
}
