using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Commands.UpdateUserPhoneNumber;

public sealed class UpdateUserPhoneNumberCommandHandler : ICommandHandler<UpdateUserPhoneNumberCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserPhoneNumberCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> HandleAsync(UpdateUserPhoneNumberCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        var updateResult = user.UpdatePhoneNumber(command.PhoneNumber);
        if (updateResult.IsFailure)
            return updateResult;

        _userRepository.Update(user);

        return Result.Success();
    }
}