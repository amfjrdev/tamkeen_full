using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Repositories;

namespace SP.Application.ServiceRequests.Commands.RejectServiceRequest;

public sealed record RejectServiceRequestCommand(
    Guid RequestId,
    string? Reason = null) : ICommand;

public sealed class RejectServiceRequestCommandValidator : AbstractValidator<RejectServiceRequestCommand>
{
    public RejectServiceRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class RejectServiceRequestCommandHandler : ICommandHandler<RejectServiceRequestCommand>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectServiceRequestCommandHandler(
        IServiceRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        RejectServiceRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure(ServiceRequestErrors.NotFound);

        var result = request.Reject(command.Reason);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
