using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SP.Domain.Payments;
using SP.Domain.Payments.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Payment?> GetByCheckoutIdAsync(string checkoutId, CancellationToken cancellationToken = default)
        => await Context.Set<Payment>()
            .FirstOrDefaultAsync(p => p.CheckoutId == checkoutId, cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetHistoryByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Set<Payment>()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
}
