using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SP.Domain.Users;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await Context.Users
            .IgnoreQueryFilters()   // allow finding blocked/deleted users during auth
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await Context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => await Context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                u => u.RefreshTokens.Any(t => t.Token == refreshToken),
                cancellationToken);

    public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await Context.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);
}
