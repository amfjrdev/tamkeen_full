using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;
using SP.Domain.ProviderProfiles.Repositories;

namespace SP.Application.Users.Commands.UpdateUserLocation;

public sealed class UpdateUserLocationCommandHandler : ICommandHandler<UpdateUserLocationCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IProviderProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserLocationCommandHandler(
        IUserRepository userRepository,
        IProviderProfileRepository profileRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateUserLocationCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        user.UpdateLocation(command.Latitude, command.Longitude);

        if (user.Role == UserRole.Provider)
        {
            var profile = await _profileRepository.GetByProviderIdAsync(user.Id, cancellationToken);
            if (profile is not null)
            {
                profile.SetLocation(command.Latitude, command.Longitude);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
