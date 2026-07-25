using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Authentication;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 8)
            return Result.Failure(UserErrors.InvalidPassword);

        if (string.IsNullOrWhiteSpace(command.Token))
            return Result.Failure(UserErrors.InvalidPasswordResetToken);

        var user = await _userRepository.GetByEmailAsync(command.Token, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.InvalidPasswordResetToken); // was: UserErrors.NotFound (404)

        var hash = _passwordHasher.Hash(command.NewPassword);
        var result = user.UpdatePassword(hash);
        if (result.IsFailure)
            return result;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // was: missing
        return Result.Success();
    }
}