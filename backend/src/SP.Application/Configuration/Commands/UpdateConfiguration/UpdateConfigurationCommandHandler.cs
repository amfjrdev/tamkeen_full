using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Shared;

namespace SP.Application.Configuration.Commands.UpdateConfiguration;

public sealed class UpdateConfigurationCommandHandler : ICommandHandler<UpdateConfigurationCommand>
{
    private readonly IAppConfigurationRepository _configRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateConfigurationCommandHandler(
        IAppConfigurationRepository configRepository,
        IUnitOfWork unitOfWork)
    {
        _configRepository = configRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        UpdateConfigurationCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingConfig = await _configRepository.GetByKeyAsync(command.Key, cancellationToken);

        if (existingConfig is not null)
        {
            existingConfig.UpdateValue(command.Value);
            await _configRepository.UpdateAsync(existingConfig, cancellationToken);
        }
        else
        {
            var newConfig = AppConfiguration.Create(
                command.Key,
                command.Value,
                command.Category,
                command.IsPublic);

            await _configRepository.AddAsync(newConfig, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}