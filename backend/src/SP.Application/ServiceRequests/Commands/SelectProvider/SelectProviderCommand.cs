using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Repositories;

namespace SP.Application.ServiceRequests.Commands.SelectProvider;

public sealed record SelectProviderCommand(
    Guid RequestId,
    Guid ClientId,
    Guid ProviderId,
    Guid ApplicationId) : ICommand;

public sealed class SelectProviderCommandValidator : AbstractValidator<SelectProviderCommand>
{
    public SelectProviderCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ApplicationId).NotEmpty();
    }
}

public sealed class SelectProviderCommandHandler : ICommandHandler<SelectProviderCommand>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SelectProviderCommandHandler(
        IServiceRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        SelectProviderCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdWithApplicationsAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure(ServiceRequestErrors.NotFound);

        if (request.ClientId != command.ClientId)
            return Result.Failure(ServiceRequestErrors.UnauthorizedAction);

        var result = request.SelectProvider(command.ProviderId, command.ApplicationId);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
