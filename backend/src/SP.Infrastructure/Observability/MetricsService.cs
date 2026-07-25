using SP.Application.Abstractions.Metrics;

namespace SP.Infrastructure.Observability;

public sealed class MetricsService : IMetricsService
{
    public void IncrementFailedPayments()
    {
        EnterpriseMetrics.FailedPaymentsCounter.Add(1);
    }

    public void IncrementChatMessages()
    {
        EnterpriseMetrics.ChatMessagesCounter.Add(1);
    }

    public void IncrementWalletTransactions()
    {
        EnterpriseMetrics.WalletTransactionsCounter.Add(1);
    }
}
