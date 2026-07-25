using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.ProviderProfiles.Errors;
using SP.Domain.ProviderProfiles.Repositories;

namespace SP.Application.Providers.Commands.UpdateAvailability;

public sealed record UpdateAvailabilityCommand(
    Guid ProviderId,
    bool IsAvailable) : ICommand;

public sealed class UpdateAvailabilityCommandHandler
    : ICommandHandler<UpdateAvailabilityCommand>
{
    private readonly IProviderProfileRepository _providerProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAvailabilityCommandHandler(
        IProviderProfileRepository providerProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _providerProfileRepository = providerProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        UpdateAvailabilityCommand command,
        CancellationToken cancellationToken = default)
    {
        var profile = await _providerProfileRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ProviderProfileErrors.NotFound);
        }

        var result = profile.SetAvailability(command.IsAvailable);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        _providerProfileRepository.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
