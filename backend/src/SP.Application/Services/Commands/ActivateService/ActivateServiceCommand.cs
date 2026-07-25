// SP.Application/Services/Commands/ActivateService/ActivateServiceCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Services.Errors;
using SP.Domain.Services.Repositories;

namespace SP.Application.Services.Commands.ActivateService;

public sealed record ActivateServiceCommand(Guid ServiceId, Guid ProviderId) : ICommand;

public sealed class ActivateServiceCommandHandler : ICommandHandler<ActivateServiceCommand>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(ActivateServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ServiceErrors.ServiceNotFound);

        if (service.ProviderId != command.ProviderId)
            return Result.Failure(ServiceErrors.InvalidProviderId);

        var result = service.Activate();
        if (result.IsFailure) return result;

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
