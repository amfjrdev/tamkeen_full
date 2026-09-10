using SP.Domain.Abstractions;

namespace SP.Domain.Categories.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task HardDeleteAsync(Guid categoryId, CancellationToken ct = default);
}