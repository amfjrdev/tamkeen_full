using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Commands.DeleteUserAccount;

public sealed class DeleteUserAccountCommandHandler : ICommandHandler<DeleteUserAccountCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserAccountCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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

        // Infrastructure layer will handle:
        // - Password verification
        // - Cascading deletes (bookings, reviews, etc.)
        // - Data anonymization where required
        
        // Mark user as deleted
        var deleteResult = user.Delete();
        if (deleteResult.IsFailure)
            return deleteResult;

        _userRepository.Update(user);

        return Result.Success();
    }
}