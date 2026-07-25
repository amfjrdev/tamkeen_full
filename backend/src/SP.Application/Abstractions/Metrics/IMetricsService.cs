namespace SP.Application.Abstractions.Metrics;

public interface IMetricsService
{
    void IncrementFailedPayments();
    void IncrementChatMessages();
    void IncrementWalletTransactions();
}
