namespace SP.Infrastructure.Payments.Chargily;

public sealed class ChargilyOptions
{
    public const string SectionName = "Chargily";

    public string SecretKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://pay.chargily.net/test/api/v2";
    public bool AllowTestMode { get; init; } = false;
    public string SuccessUrl { get; init; } = string.Empty;
    public string FailureUrl { get; init; } = string.Empty;
}

