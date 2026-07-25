using SP.Application.Abstractions.Messaging;
using SP.Application.Users.Dto;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Queries.GetUserByEmail;

public sealed class GetUserByEmailQueryHandler : IQueryHandler<GetUserByEmailQuery, UserResponseDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserResponseDto>> HandleAsync(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(query.Email, cancellationToken);
        if (user is null)
            return Result.Failure<UserResponseDto>(UserErrors.NotFound);

        var response = user.ToResponse();

        return Result.Success(response);
    }
}