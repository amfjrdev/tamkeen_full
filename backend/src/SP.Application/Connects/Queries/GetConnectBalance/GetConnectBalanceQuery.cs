using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Connects.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Connects.Repositories;

namespace SP.Application.Connects.Queries.GetConnectBalance;

public sealed record GetConnectBalanceQuery(Guid UserId) : IQuery<ConnectBalanceResponse>;

public sealed class GetConnectBalanceQueryHandler
    : IQueryHandler<GetConnectBalanceQuery, ConnectBalanceResponse>
{
    private readonly IWalletRepository _walletRepository;

    public GetConnectBalanceQueryHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<ConnectBalanceResponse>> HandleAsync(
        GetConnectBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        var balance = wallet?.Balance ?? 0;
        return Result.Success(new ConnectBalanceResponse(balance));
    }
}
