using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Connects.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Connects.Repositories;

namespace SP.Application.Connects.Queries.GetConnectPacks;

public sealed record GetConnectPacksQuery() : IQuery<IReadOnlyList<ConnectPackDto>>;

public sealed class GetConnectPacksQueryHandler
    : IQueryHandler<GetConnectPacksQuery, IReadOnlyList<ConnectPackDto>>
{
    private readonly IConnectPackRepository _connectPackRepository;

    public GetConnectPacksQueryHandler(IConnectPackRepository connectPackRepository)
    {
        _connectPackRepository = connectPackRepository;
    }

    public async Task<Result<IReadOnlyList<ConnectPackDto>>> HandleAsync(
        GetConnectPacksQuery query,
        CancellationToken cancellationToken = default)
    {
        var packs = await _connectPackRepository.GetAllAsync(cancellationToken);
        
        var activePacks = packs
            .Where(p => p.Status == "active")
            .Select(p => new ConnectPackDto(p.Code, p.Name, p.Credits, p.Price, "DZD", p.IsPopular))
            .ToList();

        return Result.Success<IReadOnlyList<ConnectPackDto>>(activePacks);
    }
}
