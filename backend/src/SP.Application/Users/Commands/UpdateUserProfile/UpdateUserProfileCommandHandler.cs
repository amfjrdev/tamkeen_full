using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateUserProfileCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        var updateResult = user.UpdateProfile(
            command.FirstName,
            command.LastName,
            command.PhoneNumber);

        if (updateResult.IsFailure)
            return updateResult;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}