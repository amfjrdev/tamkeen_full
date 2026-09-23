using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Chat;
using SP.Domain.Chat.Repositories;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Repositories;

namespace SP.Application.ServiceRequests.Commands.ApplyToServiceRequest;

public sealed record ApplyToServiceRequestCommand(
    Guid RequestId,
    Guid ProviderId,
    string ProviderWilaya,
    string CoverLetter,
    decimal? ProposedPrice,
    int ConnectsCost = 10) : ICommand<Guid>;

public sealed class ApplyToServiceRequestCommandValidator : AbstractValidator<ApplyToServiceRequestCommand>
{
    public ApplyToServiceRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.CoverLetter).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ProposedPrice).GreaterThanOrEqualTo(0).When(x => x.ProposedPrice.HasValue);
    }
}

public sealed class ApplyToServiceRequestCommandHandler : ICommandHandler<ApplyToServiceRequestCommand, Guid>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IConnectTransactionRepository _transactionRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyToServiceRequestCommandHandler(
        IServiceRequestRepository requestRepository,
        IWalletRepository walletRepository,
        IConnectTransactionRepository transactionRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(
        ApplyToServiceRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdWithApplicationsAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure<Guid>(ServiceRequestErrors.NotFound);

        var wilaya = string.IsNullOrWhiteSpace(command.ProviderWilaya)
            ? request.Wilaya
            : command.ProviderWilaya.Trim();

        var idempotencyKey = $"apply-sr-{command.RequestId}-{command.ProviderId}";
        var existingTx = await _transactionRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existingTx is not null)
            return Result.Failure<Guid>(ServiceRequestErrors.AlreadyApplied);

        // 1. Debit Connects safely
        var wallet = await _walletRepository.GetByUserIdAsync(command.ProviderId, cancellationToken);
        if (wallet is null)
        {
            wallet = Wallet.Create(command.ProviderId, initialBalance: 0);
            await _walletRepository.AddAsync(wallet, cancellationToken);
        }

        var debitResult = wallet.Debit(command.ConnectsCost);
        if (debitResult.IsFailure)
            return Result.Failure<Guid>(ServiceRequestErrors.InsufficientConnects);

        var transaction = ConnectTransaction.Create(
            command.ProviderId,
            -command.ConnectsCost,
            "ServiceRequestApplication",
            idempotencyKey,
            command.RequestId);

        await _transactionRepository.AddAsync(transaction, cancellationToken);

        // 2. Add application to aggregate
        var appResult = request.AddApplication(
            command.ProviderId,
            wilaya,
            command.CoverLetter,
            command.ProposedPrice,
            command.ConnectsCost);

        if (appResult.IsFailure)
            return Result.Failure<Guid>(appResult.Error);

        // 3. Ensure conversation is ready for messaging between Client and Provider
        var existingConv = await _conversationRepository.GetByParticipantsAsync(
            request.ClientId, command.ProviderId, cancellationToken);

        if (existingConv is null)
        {
            var newConv = Conversation.Create(request.ClientId, command.ProviderId, unlockCost: 0);
            newConv.Unlock();
            await _conversationRepository.AddAsync(newConv, cancellationToken);
        }
        else if (existingConv.IsLocked)
        {
            existingConv.Unlock();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(appResult.Value.Id);
    }
}
