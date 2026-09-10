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

    public async Task HardDeleteAsync(Guid categoryId, CancellationToken ct = default)
    {
        var services = await Context.Services.Where(s => s.CategoryId == categoryId).ToListAsync(ct);
        if (services.Any())
        {
            var serviceIds = services.Select(s => s.Id).ToList();
            var bookings = await Context.Bookings.Where(b => serviceIds.Contains(b.ServiceId)).ToListAsync(ct);
            if (bookings.Any())
            {
                Context.Bookings.RemoveRange(bookings);
            }
            Context.Services.RemoveRange(services);
        }

        var category = await Context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
        if (category is not null)
        {
            Context.Categories.Remove(category);
        }
    }
}
