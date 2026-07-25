// SP.Infrastructure/Persistence/Repositories/CategoryRepository.cs

using Microsoft.EntityFrameworkCore;
using SP.Domain.Categories;
using SP.Domain.Categories.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await Context.Categories
            .AnyAsync(c => c.Name.ToLower() == name.Trim().ToLower(), ct);
}
