using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Payments;
using SP.Domain.Abstractions;
using SP.Domain.Payments;
using SP.Domain.Payments.Repositories;

using SP.Domain.Connects.Repositories;

namespace SP.Application.Payments.Commands.CreateCheckoutSession;

public sealed record CreateCheckoutSessionCommand(
    Guid UserId,
    string PackId,
    string SuccessUrl,
    string FailureUrl) : ICommand<CheckoutSessionResponse>;

public sealed record CheckoutSessionResponse(string CheckoutUrl, string CheckoutId);

public sealed class CreateCheckoutSessionCommandHandler
    : ICommandHandler<CreateCheckoutSessionCommand, CheckoutSessionResponse>
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConnectPackRepository _connectPackRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCheckoutSessionCommandHandler(
        IPaymentGateway paymentGateway,
        IPaymentRepository paymentRepository,
        IConnectPackRepository connectPackRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentGateway = paymentGateway;
        _paymentRepository = paymentRepository;
        _connectPackRepository = connectPackRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CheckoutSessionResponse>> HandleAsync(
        CreateCheckoutSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedPackId = command.PackId.ToLowerInvariant().Trim();
        var pack = await _connectPackRepository.GetByCodeAsync(normalizedPackId, cancellationToken);
        if (pack is null)
        {
            return Result.Failure<CheckoutSessionResponse>(
                new Error("ConnectPack.NotFound", "The specified connect package was not found."));
        }

        // Call the payment gateway to initialize the checkout session
        var gatewayResult = await _paymentGateway.CreateCheckoutAsync(
            command.UserId,
            pack.Price,
            normalizedPackId,
            command.SuccessUrl,
            command.FailureUrl,
            cancellationToken);

        if (gatewayResult.IsFailure)
        {
            return Result.Failure<CheckoutSessionResponse>(gatewayResult.Error);
        }

        var checkoutResponse = gatewayResult.Value;

        // Save a Pending Payment record in the local database
        var payment = Payment.Create(
            command.UserId,
            checkoutResponse.CheckoutId,
            checkoutResponse.CheckoutUrl,
            normalizedPackId,
            pack.Price);

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CheckoutSessionResponse(
            checkoutResponse.CheckoutUrl,
            checkoutResponse.CheckoutId));
    }
}
