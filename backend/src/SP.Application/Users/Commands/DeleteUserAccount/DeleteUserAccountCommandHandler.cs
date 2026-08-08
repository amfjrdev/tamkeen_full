using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Commands.DeleteUserAccount;

public sealed class DeleteUserAccountCommandHandler : ICommandHandler<DeleteUserAccountCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserAccountCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(DeleteUserAccountCommand command, CancellationToken cancellationToken = default)
    {
        // Validate confirmation text
        if (command.ConfirmationText != "DELETE")
            return Result.Failure(UserErrors.InvalidConfirmationText);

        // Get user
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        // Verify password
        if (user.Credential is null || !_passwordHasher.Verify(user.Credential.PasswordHash, command.Password))
            return Result.Failure(UserErrors.InvalidPassword);

        // Perform hard delete of user and all related records
        await _userRepository.HardDeleteAsync(command.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}