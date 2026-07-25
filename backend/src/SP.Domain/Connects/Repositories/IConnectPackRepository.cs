using System.Threading;
using System.Threading.Tasks;
using SP.Domain.Abstractions;

namespace SP.Domain.Connects.Repositories;

public interface IConnectPackRepository : IRepository<ConnectPack>
{
    Task<ConnectPack?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ConnectPack?> GetByIntIdAsync(int intId, CancellationToken cancellationToken = default);
}
