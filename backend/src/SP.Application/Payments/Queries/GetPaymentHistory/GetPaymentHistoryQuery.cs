using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Payments.Repositories;

namespace SP.Application.Payments.Queries.GetPaymentHistory;

public sealed record GetPaymentHistoryQuery(Guid UserId) : IQuery<IReadOnlyList<PaymentHistoryDto>>;

public sealed record PaymentHistoryDto(
    Guid Id,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    string PackId);

public sealed class GetPaymentHistoryQueryHandler
    : IQueryHandler<GetPaymentHistoryQuery, IReadOnlyList<PaymentHistoryDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentHistoryQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<IReadOnlyList<PaymentHistoryDto>>> HandleAsync(
        GetPaymentHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetHistoryByUserIdAsync(query.UserId, cancellationToken);

        var history = payments.Select(p => new PaymentHistoryDto(
            p.Id,
            p.Amount,
            p.Status.ToString(),
            p.CreatedAt,
            p.PackId ?? string.Empty
        )).ToList();

        return Result.Success<IReadOnlyList<PaymentHistoryDto>>(history);
    }
}
