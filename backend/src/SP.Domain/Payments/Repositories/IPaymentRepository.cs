using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SP.Domain.Abstractions;

namespace SP.Domain.Payments.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByCheckoutIdAsync(string checkoutId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetHistoryByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
