using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Repositories;
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
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnlockConversationCommandHandler(
        IConversationRepository conversationRepository,
        IWalletRepository walletRepository,
        IConnectTransactionRepository transactionRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _bookingRepository = bookingRepository;
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

        // Automatically accept the pending booking request when unlocking the conversation
        var clientUserId = conversation.Participant1Id == command.UserId
            ? conversation.Participant2Id
            : conversation.Participant1Id;

        var clientBookings = await _bookingRepository.GetByClientIdAsync(clientUserId, cancellationToken);
        var pendingBooking = clientBookings.FirstOrDefault(b => b.ProviderId == command.UserId && b.Status == BookingStatus.Pending);
        if (pendingBooking != null)
        {
            var acceptResult = pendingBooking.Accept();
            if (acceptResult.IsSuccess)
            {
                _bookingRepository.Update(pendingBooking);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
