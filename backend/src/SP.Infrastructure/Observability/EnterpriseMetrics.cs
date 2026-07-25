using System.Diagnostics.Metrics;

namespace SP.Infrastructure.Observability;

public static class EnterpriseMetrics
{
    private static readonly Meter Meter = new("SP.API", "1.0.0");

    public static readonly Counter<long> FailedPaymentsCounter = Meter.CreateCounter<long>(
        "sp_failed_payments_total",
        description: "Total number of failed payment attempts");

    public static readonly Counter<long> ChatMessagesCounter = Meter.CreateCounter<long>(
        "sp_chat_messages_total",
        description: "Total number of chat messages sent");

    public static readonly Counter<long> WalletTransactionsCounter = Meter.CreateCounter<long>(
        "sp_wallet_transactions_total",
        description: "Total number of wallet transactions (credits/debits)");
}
