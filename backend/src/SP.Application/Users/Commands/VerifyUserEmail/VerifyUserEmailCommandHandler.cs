using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Commands.VerifyUserEmail;

public sealed class VerifyUserEmailCommandHandler : ICommandHandler<VerifyUserEmailCommand>
{
    private readonly IUserRepository _userRepository;

    public VerifyUserEmailCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> HandleAsync(VerifyUserEmailCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        var verifyResult = user.VerifyEmail();
        if (verifyResult.IsFailure)
            return verifyResult;

        _userRepository.Update(user);

        return Result.Success();
    }
}