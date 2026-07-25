using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Domain.Abstractions;

namespace SP.Application.Abstractions.Payments;

public interface IPaymentGateway
{
    Task<Result<CheckoutResponse>> CreateCheckoutAsync(
        Guid userId,
        decimal amount,
        string packId,
        string successUrl,
        string failureUrl,
        CancellationToken cancellationToken = default);
}

public sealed record CheckoutResponse(string CheckoutId, string CheckoutUrl);
