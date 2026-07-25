using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Chat.Errors;
using SP.Domain.Chat.Repositories;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;

namespace SP.Application.Chat.Commands.UnlockConversation;

public sealed record UnlockConversationCommand(
    Guid UserId,
    Guid ConversationId) : ICommand;

public sealed class UnlockConversationCommandHandler
    : ICommandHandler<UnlockConversationCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IConnectTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnlockConversationCommandHandler(
        IConversationRepository conversationRepository,
        IWalletRepository walletRepository,
        IConnectTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        UnlockConversationCommand command,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(command.ConversationId, cancellationToken);
        if (conversation is null)
            return Result.Failure(ChatErrors.ConversationNotFound);

        if (conversation.Participant1Id != command.UserId && conversation.Participant2Id != command.UserId)
            return Result.Failure(ChatErrors.Unauthorized);

        if (!conversation.IsLocked)
            return Result.Failure(ChatErrors.AlreadyUnlocked);

        // Idempotency check
        var idempotencyKey = $"unlock-{command.ConversationId}";
        var existingTx = await _transactionRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existingTx is not null)
        {
            conversation.Unlock();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        var wallet = await _walletRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (wallet is null)
        {
            wallet = Wallet.Create(command.UserId, initialBalance: 0);
            await _walletRepository.AddAsync(wallet, cancellationToken);
        }

        var debitResult = wallet.Debit(conversation.UnlockCost);
        if (debitResult.IsFailure)
            return Result.Failure(ChatErrors.InsufficientBalance);

        var transaction = ConnectTransaction.Create(
            command.UserId,
            -conversation.UnlockCost,
            "UnlockConversation",
            idempotencyKey,
            conversation.Id);

        await _transactionRepository.AddAsync(transaction, cancellationToken);

        conversation.Unlock();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
