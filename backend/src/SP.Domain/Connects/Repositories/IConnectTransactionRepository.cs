using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Domain.Abstractions;

namespace SP.Domain.Connects.Repositories;

public interface IConnectTransactionRepository : IRepository<ConnectTransaction>
{
    Task<ConnectTransaction?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
}
