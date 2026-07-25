using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class WalletRepository : Repository<Wallet>, IWalletRepository
{
    public WalletRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Set<Wallet>()
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
}
