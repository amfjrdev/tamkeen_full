using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Repositories;

namespace SP.Application.ServiceRequests.Commands.CompleteServiceRequest;

public sealed record CompleteServiceRequestCommand(
    Guid RequestId,
    Guid ClientId) : ICommand;

public sealed class CompleteServiceRequestCommandValidator : AbstractValidator<CompleteServiceRequestCommand>
{
    public CompleteServiceRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty();
    }
}

public sealed class CompleteServiceRequestCommandHandler : ICommandHandler<CompleteServiceRequestCommand>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteServiceRequestCommandHandler(
        IServiceRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        CompleteServiceRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure(ServiceRequestErrors.NotFound);

        if (request.ClientId != command.ClientId)
            return Result.Failure(ServiceRequestErrors.UnauthorizedAction);

        var result = request.Complete();
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
