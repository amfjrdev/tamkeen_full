using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Commands.UnblockUser;

public sealed class UnblockUserCommandHandler : ICommandHandler<UnblockUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnblockUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UnblockUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        var unblockResult = user.Unblock();
        if (unblockResult.IsFailure)
            return Result.Failure(unblockResult.Error);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}