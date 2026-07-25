using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class ConnectPackRepository : Repository<ConnectPack>, IConnectPackRepository
{
    public ConnectPackRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ConnectPack?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();
        return await Context.Set<ConnectPack>()
            .FirstOrDefaultAsync(p => p.Code == normalizedCode, cancellationToken);
    }

    public async Task<ConnectPack?> GetByIntIdAsync(int intId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ConnectPack>()
            .FirstOrDefaultAsync(p => p.IntId == intId, cancellationToken);
    }
}
