// SP.Application/Services/Commands/CreateService/CreateServiceCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Services;
using SP.Domain.Services.Errors;
using SP.Domain.Services.Repositories;

namespace SP.Application.Services.Commands.CreateService;

public sealed record CreateServiceCommand(
    Guid ProviderId,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    int DurationMinutes) : ICommand<Guid>;

public sealed class CreateServiceCommandHandler : ICommandHandler<CreateServiceCommand, Guid>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(CreateServiceCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await _serviceRepository.ExistsForProviderAsync(command.ProviderId, command.Name, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(ServiceErrors.InvalidServiceName);

        var serviceResult = Service.Create(
            command.ProviderId,
            command.CategoryId,
            command.Name,
            command.Description,
            command.Price,
            command.DurationMinutes);

        if (serviceResult.IsFailure)
            return Result.Failure<Guid>(serviceResult.Error);

        await _serviceRepository.AddAsync(serviceResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(serviceResult.Value.Id);
    }
}
