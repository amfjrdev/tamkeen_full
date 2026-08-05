using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using SP.Application.Abstractions.Payments;
using SP.Application.Payments.Commands.CompletePayment;
using SP.Application.Payments.Commands.CreateCheckoutSession;
using SP.Application.Payments.Queries.GetPaymentHistory;
using SP.Domain.Abstractions;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;
using SP.Domain.Payments;
using SP.Domain.Payments.Enums;
using SP.Domain.Payments.Errors;
using SP.Domain.Payments.Repositories;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SP.UnitTests.Application.Payments;

public sealed class PaymentCommandHandlerTests
{
    private readonly IPaymentGateway _paymentGateway = Substitute.For<IPaymentGateway>();
    private readonly IPaymentRepository _paymentRepo = Substitute.For<IPaymentRepository>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IConnectTransactionRepository _transactionRepo = Substitute.For<IConnectTransactionRepository>();
    private readonly IConnectPackRepository _connectPackRepo = Substitute.For<IConnectPackRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CompletePaymentCommandHandler> _logger = Substitute.For<ILogger<CompletePaymentCommandHandler>>();

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly string CheckoutId = "checkout-123";
    private static readonly string CheckoutUrl = "https://pay.chargily.dz/pay/123";
    private static readonly string PackId = "starter";
    private static readonly decimal Amount = 500.00m;

    // ── CreateCheckoutSession ─────────────────────────────────────────────

    [Fact]
    public async Task CreateCheckoutSession_WithValidPack_ReturnsCheckoutDetails()
    {
        // Arrange
        var pack = ConnectPack.Create(PackId, "Starter Pack", "Description", Amount, 5, "blue", new List<string>(), false, 0);
        _connectPackRepo.GetByCodeAsync(PackId, Arg.Any<CancellationToken>()).Returns(pack);

        _paymentGateway.CreateCheckoutAsync(UserId, Amount, PackId, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new CheckoutResponse(CheckoutId, CheckoutUrl)));

        var handler = new CreateCheckoutSessionCommandHandler(_paymentGateway, _paymentRepo, _connectPackRepo, _uow);
        var command = new CreateCheckoutSessionCommand(UserId, PackId, "http://success", "http://failure");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CheckoutId.Should().Be(CheckoutId);
        result.Value.CheckoutUrl.Should().Be(CheckoutUrl);
        await _paymentRepo.Received(1).AddAsync(Arg.Is<Payment>(p => p.UserId == UserId && p.CheckoutId == CheckoutId && p.Amount == Amount));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCheckoutSession_WithInvalidPack_ReturnsFailure()
    {
        // Arrange
        _connectPackRepo.GetByCodeAsync("invalid-pack", Arg.Any<CancellationToken>()).Returns((ConnectPack?)null);

        var handler = new CreateCheckoutSessionCommandHandler(_paymentGateway, _paymentRepo, _connectPackRepo, _uow);
        var command = new CreateCheckoutSessionCommand(UserId, "invalid-pack", "http://success", "http://failure");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ConnectPack.NotFound");
    }

    // ── CompletePayment ───────────────────────────────────────────────────

    [Fact]
    public async Task CompletePayment_WhenPendingAndPaid_CreditsWalletAndSaves()
    {
        // Arrange
        var payment = Payment.Create(UserId, CheckoutId, CheckoutUrl, PackId, Amount);
        _paymentRepo.GetByCheckoutIdAsync(CheckoutId, Arg.Any<CancellationToken>()).Returns(payment);

        var wallet = Wallet.Create(UserId, 0);
        _walletRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(wallet);

        var handler = new CompletePaymentCommandHandler(_paymentRepo, _walletRepo, _transactionRepo, _connectPackRepo, _uow, _logger);
        var command = new CompletePaymentCommand(CheckoutId, "paid", Amount);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Paid);
        wallet.Balance.Should().Be(5); // Starter pack gives 5 connects
        await _transactionRepo.Received(1).AddAsync(Arg.Is<ConnectTransaction>(t => t.UserId == UserId && t.Amount == 5 && t.Type == "Purchase"), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompletePayment_WhenAlreadyPaid_ReturnsSuccessImmediatelyWithoutReCrediting()
    {
        // Arrange
        var payment = Payment.Create(UserId, CheckoutId, CheckoutUrl, PackId, Amount);
        payment.Complete(PaymentStatus.Paid);
        _paymentRepo.GetByCheckoutIdAsync(CheckoutId, Arg.Any<CancellationToken>()).Returns(payment);

        var handler = new CompletePaymentCommandHandler(_paymentRepo, _walletRepo, _transactionRepo, _connectPackRepo, _uow, _logger);
        var command = new CompletePaymentCommand(CheckoutId, "paid", Amount);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _walletRepo.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompletePayment_WhenPaymentNotFound_ReturnsFailure()
    {
        // Arrange
        _paymentRepo.GetByCheckoutIdAsync(CheckoutId, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var handler = new CompletePaymentCommandHandler(_paymentRepo, _walletRepo, _transactionRepo, _connectPackRepo, _uow, _logger);
        var command = new CompletePaymentCommand(CheckoutId, "paid", Amount);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.NotFound);
    }

    [Fact]
    public async Task CompletePayment_WhenPendingAndFailed_UpdatesStatusToFailed()
    {
        // Arrange
        var payment = Payment.Create(UserId, CheckoutId, CheckoutUrl, PackId, Amount);
        _paymentRepo.GetByCheckoutIdAsync(CheckoutId, Arg.Any<CancellationToken>()).Returns(payment);

        var handler = new CompletePaymentCommandHandler(_paymentRepo, _walletRepo, _transactionRepo, _connectPackRepo, _uow, _logger);
        var command = new CompletePaymentCommand(CheckoutId, "failed", Amount);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
        await _walletRepo.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompletePayment_WhenPendingAndExpired_UpdatesStatusToCancelled()
    {
        // Arrange
        var payment = Payment.Create(UserId, CheckoutId, CheckoutUrl, PackId, Amount);
        _paymentRepo.GetByCheckoutIdAsync(CheckoutId, Arg.Any<CancellationToken>()).Returns(payment);

        var handler = new CompletePaymentCommandHandler(_paymentRepo, _walletRepo, _transactionRepo, _connectPackRepo, _uow, _logger);
        var command = new CompletePaymentCommand(CheckoutId, "expired", Amount);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Cancelled);
        await _walletRepo.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── GetPaymentHistoryQuery ────────────────────────────────────────────

    [Fact]
    public async Task GetPaymentHistory_ReturnsMappedHistoryDtos()
    {
        // Arrange
        var payment = Payment.Create(UserId, CheckoutId, CheckoutUrl, PackId, Amount);
        var paymentsList = new List<Payment> { payment };
        _paymentRepo.GetHistoryByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(paymentsList);

        var handler = new GetPaymentHistoryQueryHandler(_paymentRepo);
        var query = new GetPaymentHistoryQuery(UserId);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var dto = result.Value.First();
        dto.Amount.Should().Be(Amount);
        dto.Status.Should().Be(PaymentStatus.Pending.ToString());
        dto.PackId.Should().Be(PackId);
    }

    [Fact]
    public async Task CompletePayment_WhenAmountMismatch_ReturnsFailureAndDoesNotCredit()
    {
        // Arrange
        var payment = Payment.Create(UserId, CheckoutId, CheckoutUrl, PackId, Amount);
        _paymentRepo.GetByCheckoutIdAsync(CheckoutId, Arg.Any<CancellationToken>()).Returns(payment);

        var handler = new CompletePaymentCommandHandler(_paymentRepo, _walletRepo, _transactionRepo, _connectPackRepo, _uow, _logger);
        var command = new CompletePaymentCommand(CheckoutId, "paid", Amount + 10.00m);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.AmountMismatch");
        payment.Status.Should().Be(PaymentStatus.Pending);
        await _walletRepo.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
