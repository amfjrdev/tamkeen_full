using SP.Application.Abstractions.Messaging;
using SP.Application.Users.Dto;
using SP.Domain.Abstractions;
using SP.Domain.Users;

namespace SP.Application.Users.Queries.GetProviders;

public sealed record GetProvidersQuery : IQuery<IReadOnlyList<UserResponseDto>>;

public sealed class GetProvidersQueryHandler(IUserRepository repository)
    : IQueryHandler<GetProvidersQuery, IReadOnlyList<UserResponseDto>>
{
    public async Task<Result<IReadOnlyList<UserResponseDto>>> HandleAsync(
        GetProvidersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        var providers = users
            .Where(u => u.Role == UserRole.Provider)
            .Select(u => u.ToResponse())
            .ToList();

        return Result.Success<IReadOnlyList<UserResponseDto>>(providers);
    }
}