using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SP.Application.Abstractions.Payments;
using SP.Domain.Abstractions;

namespace SP.Infrastructure.Payments.Chargily;

public sealed class ChargilyPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly ChargilyOptions _options;
    private readonly ILogger<ChargilyPaymentGateway> _logger;

    public ChargilyPaymentGateway(
        HttpClient httpClient,
        IOptions<ChargilyOptions> options,
        ILogger<ChargilyPaymentGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<CheckoutResponse>> CreateCheckoutAsync(
        Guid userId,
        decimal amount,
        string packId,
        string successUrl,
        string failureUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Chargily checkout session for user {UserId}, pack {PackId}, amount {Amount}", userId, packId, amount);

            if (!_options.AllowTestMode && _options.BaseUrl.Contains("/test/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Security Violation: BaseUrl is configured to a test environment, but test mode is disabled.");
                return Result.Failure<CheckoutResponse>(new Error("Payments.SecurityViolation", "Test payments are not allowed in this environment."));
            }

            var finalSuccessUrl = string.IsNullOrWhiteSpace(successUrl) ? _options.SuccessUrl : successUrl;
            var finalFailureUrl = string.IsNullOrWhiteSpace(failureUrl) ? _options.FailureUrl : failureUrl;

            var requestUri = $"{_options.BaseUrl.TrimEnd('/')}/checkouts";
            
            var requestBody = new
            {
                amount = (int)amount,
                currency = "dzd",
                success_url = finalSuccessUrl,
                failure_url = finalFailureUrl,
                metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "packId", packId }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.SecretKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Chargily checkout creation failed with status {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
                return Result.Failure<CheckoutResponse>(new Error("Payments.GatewayError", $"Failed to initiate payment gateway: {response.StatusCode}"));
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var id = jsonResponse.GetProperty("id").GetString();
            var checkoutUrl = jsonResponse.GetProperty("checkout_url").GetString();

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(checkoutUrl))
            {
                return Result.Failure<CheckoutResponse>(new Error("Payments.GatewayInvalidResponse", "Invalid response received from payment gateway."));
            }

            return Result.Success(new CheckoutResponse(id, checkoutUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error creating Chargily checkout session");
            return Result.Failure<CheckoutResponse>(new Error("Payments.GatewayException", "An error occurred while connecting to the payment gateway."));
        }
    }
}
