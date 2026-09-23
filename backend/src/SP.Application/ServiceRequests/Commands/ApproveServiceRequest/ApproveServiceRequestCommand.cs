using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Repositories;

namespace SP.Application.ServiceRequests.Commands.ApproveServiceRequest;

public sealed record ApproveServiceRequestCommand(Guid RequestId) : ICommand;

public sealed class ApproveServiceRequestCommandValidator : AbstractValidator<ApproveServiceRequestCommand>
{
    public ApproveServiceRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
    }
}

public sealed class ApproveServiceRequestCommandHandler : ICommandHandler<ApproveServiceRequestCommand>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveServiceRequestCommandHandler(
        IServiceRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        ApproveServiceRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure(ServiceRequestErrors.NotFound);

        var result = request.Approve();
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
