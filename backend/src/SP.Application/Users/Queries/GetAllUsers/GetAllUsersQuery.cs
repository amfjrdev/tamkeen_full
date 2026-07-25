using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Commands.Create;
using SP.Application.Users.Dto;
using SP.Domain.Abstractions;
using SP.Domain.Users;

namespace SP.Application.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery : GetAllQuery<UserResponseDto>;

public sealed class GetAllUsersQueryHandler(IUserRepository repository)
    : GetAllQueryHandler<User, UserResponseDto>(repository),
      IQueryHandler<GetAllUsersQuery, IReadOnlyList<UserResponseDto>>
{
    public Task<Result<IReadOnlyList<UserResponseDto>>> HandleAsync(
        GetAllUsersQuery query, CancellationToken cancellationToken)
        => HandleAsync((GetAllQuery<UserResponseDto>)query, cancellationToken);

    protected override UserResponseDto MapToResponse(User entity) =>
        entity.ToResponse();
}