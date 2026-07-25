using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class ConnectTransactionRepository : Repository<ConnectTransaction>, IConnectTransactionRepository
{
    public ConnectTransactionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ConnectTransaction?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default)
        => await Context.Set<ConnectTransaction>()
            .FirstOrDefaultAsync(t => t.IdempotencyKey == key, cancellationToken);
}
