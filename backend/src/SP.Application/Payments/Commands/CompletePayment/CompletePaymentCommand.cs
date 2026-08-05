using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;
using SP.Domain.Payments;
using SP.Domain.Payments.Enums;
using SP.Domain.Payments.Errors;
using SP.Domain.Payments.Repositories;

namespace SP.Application.Payments.Commands.CompletePayment;

public sealed record CompletePaymentCommand(
    string CheckoutId,
    string Status,
    decimal Amount) : ICommand;

public sealed class CompletePaymentCommandHandler : ICommandHandler<CompletePaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IConnectTransactionRepository _transactionRepository;
    private readonly IConnectPackRepository _connectPackRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompletePaymentCommandHandler> _logger;

    private static readonly Dictionary<string, int> ConnectsMap = new()
    {
        { "starter", 5 },
        { "medium", 15 },
        { "premium", 50 }
    };

    public CompletePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IWalletRepository walletRepository,
        IConnectTransactionRepository transactionRepository,
        IConnectPackRepository connectPackRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompletePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _connectPackRepository = connectPackRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(
        CompletePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch Payment from Database
        var payment = await _paymentRepository.GetByCheckoutIdAsync(command.CheckoutId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound);
        }

        // 2. Check if Payment is already processed (for idempotency)
        if (payment.Status != PaymentStatus.Pending)
        {
            return Result.Success();
        }

        // 2.5. Verify Payment Amount Integrity
        if (payment.Amount != command.Amount)
        {
            _logger.LogWarning("Security Violation: Payment amount mismatch for checkout {CheckoutId}. Expected: {Expected}, Paid: {Paid}", command.CheckoutId, payment.Amount, command.Amount);
            return Result.Failure(new Error("Payment.AmountMismatch", "The paid amount does not match the package price."));
        }

        // 3. Resolve status transition
        var targetStatus = ResolvePaymentStatus(command.Status);

        // 4. Update payment status
        var completeResult = payment.Complete(targetStatus);
        if (completeResult.IsFailure)
        {
            return Result.Failure(completeResult.Error);
        }

        // 5. If Paid, credit connects to user's wallet
        if (targetStatus == PaymentStatus.Paid)
        {
            var packId = payment.PackId?.ToLowerInvariant().Trim() ?? string.Empty;
            int connectsAmount = 0;

            var pack = await _connectPackRepository.GetByCodeAsync(packId, cancellationToken);
            if (pack is not null)
            {
                connectsAmount = pack.Credits;
            }
            else if (ConnectsMap.TryGetValue(packId, out var mappedAmount))
            {
                connectsAmount = mappedAmount;
            }
            else
            {
                return Result.Failure(new Error("Payment.InvalidPack", $"Payment references an invalid package: '{packId}'."));
            }

            // Get or create wallet
            var wallet = await _walletRepository.GetByUserIdAsync(payment.UserId, cancellationToken);
            if (wallet is null)
            {
                wallet = Wallet.Create(payment.UserId, initialBalance: 0);
                await _walletRepository.AddAsync(wallet, cancellationToken);
            }

            // Credit the wallet
            var creditResult = wallet.Credit(connectsAmount);
            if (creditResult.IsFailure)
            {
                return Result.Failure(creditResult.Error);
            }

            // Record transaction history with database-enforced idempotency key
            var transaction = ConnectTransaction.Create(
                payment.UserId,
                connectsAmount,
                "Purchase",
                $"payment-{payment.CheckoutId}",
                payment.Id);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
        }

        // 6. Persist atomic changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PaymentStatus ResolvePaymentStatus(string? rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return PaymentStatus.Failed;
        }

        return rawStatus.ToLowerInvariant().Trim() switch
        {
            "paid" => PaymentStatus.Paid,
            "failed" => PaymentStatus.Failed,
            "canceled" => PaymentStatus.Cancelled,
            "expired" => PaymentStatus.Cancelled,
            _ => PaymentStatus.Failed
        };
    }

}
