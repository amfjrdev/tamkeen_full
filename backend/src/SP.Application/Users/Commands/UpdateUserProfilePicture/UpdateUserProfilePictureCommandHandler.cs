using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;
using SP.Domain.Shared;

namespace SP.Application.Users.Commands.UpdateUserProfilePicture;

public sealed class UpdateUserProfilePictureCommandHandler : ICommandHandler<UpdateUserProfilePictureCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfilePictureCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateUserProfilePictureCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        // We bypass Uri.IsWellFormedUriString validation by using direct constructor to support relative paths
        var newImage = new Image(command.PictureUrl, true);
        user.UpdateProfilePicture(newImage);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
