using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Repositories;

namespace SP.Application.ServiceRequests.Commands.ReviewServiceRequest;

public sealed record ReviewServiceRequestCommand(
    Guid RequestId,
    Guid ClientId,
    int Rating,
    string Comment) : ICommand;

public sealed class ReviewServiceRequestCommandValidator : AbstractValidator<ReviewServiceRequestCommand>
{
    public ReviewServiceRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public sealed class ReviewServiceRequestCommandHandler : ICommandHandler<ReviewServiceRequestCommand>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewServiceRequestCommandHandler(
        IServiceRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        ReviewServiceRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure(ServiceRequestErrors.NotFound);

        if (request.ClientId != command.ClientId)
            return Result.Failure(ServiceRequestErrors.UnauthorizedAction);

        var result = request.AddReview(command.ClientId, command.Rating, command.Comment);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
