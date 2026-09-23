using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using SP.Application.Abstractions.Messaging;
using SP.Application.ServiceRequests.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Categories.Repositories;
using SP.Domain.ServiceRequests;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Repositories;

namespace SP.Application.ServiceRequests.Commands.CreateServiceRequest;

public sealed record CreateServiceRequestCommand(
    Guid ClientId,
    Guid CategoryId,
    string Title,
    string Description,
    string Wilaya,
    decimal? Budget) : ICommand<Guid>;

public sealed class CreateServiceRequestCommandValidator : AbstractValidator<CreateServiceRequestCommand>
{
    public CreateServiceRequestCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Wilaya).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Budget).GreaterThanOrEqualTo(0).When(x => x.Budget.HasValue);
    }
}

public sealed class CreateServiceRequestCommandHandler : ICommandHandler<CreateServiceRequestCommand, Guid>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceRequestCommandHandler(
        IServiceRequestRepository requestRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateServiceRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<Guid>(ServiceRequestErrors.InvalidCategory);

        var requestResult = ServiceRequest.Create(
            command.ClientId,
            command.CategoryId,
            command.Title,
            command.Description,
            command.Wilaya,
            command.Budget);

        if (requestResult.IsFailure)
            return Result.Failure<Guid>(requestResult.Error);

        var serviceRequest = requestResult.Value;
        await _requestRepository.AddAsync(serviceRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(serviceRequest.Id);
    }
}
