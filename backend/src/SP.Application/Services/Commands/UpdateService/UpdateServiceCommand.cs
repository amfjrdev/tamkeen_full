// SP.Application/Services/Commands/UpdateService/UpdateServiceCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Services.Errors;
using SP.Domain.Services.Repositories;

namespace SP.Application.Services.Commands.UpdateService;

public sealed record UpdateServiceCommand(
    Guid ServiceId,
    Guid ProviderId,
    string Name,
    string Description,
    decimal Price,
    int DurationMinutes) : ICommand;

public sealed class UpdateServiceCommandHandler : ICommandHandler<UpdateServiceCommand>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ServiceErrors.ServiceNotFound);

        if (service.ProviderId != command.ProviderId)
            return Result.Failure(ServiceErrors.InvalidProviderId);

        var priceResult = service.UpdatePrice(command.Price);
        if (priceResult.IsFailure) return priceResult;

        var durationResult = service.UpdateDuration(command.DurationMinutes);
        if (durationResult.IsFailure) return durationResult;

        var descResult = service.UpdateDescription(command.Description);
        if (descResult.IsFailure) return descResult;

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
