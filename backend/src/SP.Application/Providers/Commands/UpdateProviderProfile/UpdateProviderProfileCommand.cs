using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.ProviderProfiles;
using SP.Domain.ProviderProfiles.Repositories;

namespace SP.Application.Providers.Commands.UpdateProviderProfile;

public sealed record UpdateProviderProfileCommand(
    Guid ProviderId,
    string? AboutMe,
    decimal HourlyRate,
    int ResponseTimeMins) : ICommand;

public sealed class UpdateProviderProfileCommandHandler
    : ICommandHandler<UpdateProviderProfileCommand>
{
    private readonly IProviderProfileRepository _providerProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProviderProfileCommandHandler(
        IProviderProfileRepository providerProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _providerProfileRepository = providerProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        UpdateProviderProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var profile = await _providerProfileRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);

        var isNew = false;
        if (profile is null)
        {
            // Lazily create the provider profile the first time a provider edits it.
            profile = ProviderProfile.Create(command.ProviderId);
            isNew = true;
        }

        var result = profile.UpdateDetails(command.AboutMe, command.HourlyRate, command.ResponseTimeMins);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        if (isNew)
        {
            await _providerProfileRepository.AddAsync(profile, cancellationToken);
        }
        else
        {
            _providerProfileRepository.Update(profile);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
