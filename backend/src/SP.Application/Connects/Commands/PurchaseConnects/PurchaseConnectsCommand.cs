using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Logging;
using SP.Application.Abstractions.Metrics;
using SP.Application.Connects.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;

namespace SP.Application.Connects.Commands.PurchaseConnects;

public sealed record PurchaseConnectsCommand(
    Guid UserId,
    string PackId,
    string IdempotencyKey) : ICommand<ConnectBalanceResponse>;

public sealed class PurchaseConnectsCommandHandler
    : ICommandHandler<PurchaseConnectsCommand, ConnectBalanceResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IConnectTransactionRepository _transactionRepository;
    private readonly IConnectPackRepository _connectPackRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;
    private readonly IMetricsService _metricsService;

    public PurchaseConnectsCommandHandler(
        IWalletRepository walletRepository,
        IConnectTransactionRepository transactionRepository,
        IConnectPackRepository connectPackRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger auditLogger,
        IMetricsService metricsService)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _connectPackRepository = connectPackRepository;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
        _metricsService = metricsService;
    }

    public async Task<Result<ConnectBalanceResponse>> HandleAsync(
        PurchaseConnectsCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Idempotency Check
        var existingTx = await _transactionRepository.GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
        if (existingTx is not null)
        {
            // Transaction already exists. Return the current balance directly to satisfy idempotency safety!
            var currentWallet = await _walletRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            return Result.Success(new ConnectBalanceResponse(currentWallet?.Balance ?? 0));
        }

        // 2. Resolve Pack ID
        var normalizedPackId = command.PackId.ToLowerInvariant().Trim();
        var pack = await _connectPackRepository.GetByCodeAsync(normalizedPackId, cancellationToken);
        if (pack is null)
        {
            return Result.Failure<ConnectBalanceResponse>(new Error("ConnectPack.NotFound", "The specified connect package was not found."));
        }

        var connectsAmount = pack.Credits;

        // 3. Get or lazily create user's wallet
        var wallet = await _walletRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (wallet is null)
        {
            wallet = Wallet.Create(command.UserId, initialBalance: 0);
            await _walletRepository.AddAsync(wallet, cancellationToken);
        }

        // 4. Credit wallet
        var creditResult = wallet.Credit(connectsAmount);
        if (creditResult.IsFailure)
        {
            return Result.Failure<ConnectBalanceResponse>(creditResult.Error);
        }

        // 5. Store transaction history
        var transaction = ConnectTransaction.Create(
            command.UserId,
            connectsAmount,
            "Purchase",
            command.IdempotencyKey);

        await _transactionRepository.AddAsync(transaction, cancellationToken);

        // 6. Save transactional changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _auditLogger.Log("WalletCredit", command.UserId, new { reason = "PurchaseConnects", amount = connectsAmount, newBalance = wallet.Balance });
        _metricsService.IncrementWalletTransactions();

        return Result.Success(new ConnectBalanceResponse(wallet.Balance));
    }
}
