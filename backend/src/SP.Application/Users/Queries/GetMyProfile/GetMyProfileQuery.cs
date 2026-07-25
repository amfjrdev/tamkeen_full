using SP.Application.Abstractions.Messaging;
using SP.Application.Users.Dto;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Queries.GetMyProfile;

public sealed record GetMyProfileQuery(Guid UserId) : IQuery<UserResponseDto>;

public sealed class GetMyProfileQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetMyProfileQuery, UserResponseDto>
{
    public async Task<Result<UserResponseDto>> HandleAsync(
        GetMyProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserResponseDto>(UserErrors.NotFound);
        }

        return Result.Success(user.ToResponse());
    }
}
