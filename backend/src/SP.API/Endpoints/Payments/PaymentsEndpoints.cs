using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Payments.Commands.CompletePayment;
using SP.Application.Payments.Queries.GetPaymentHistory;
using SP.Infrastructure.Payments.Chargily;

namespace SP.API.Endpoints.Payments;

public static class PaymentsEndpoints
{
    public static RouteGroupBuilder MapPaymentsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/webhook/chargily", ReceiveWebhook);
        group.MapGet("/history", GetHistory).RequireAuthorization();

        return group;
    }

    private static async Task<IResult> ReceiveWebhook(
        HttpContext httpContext,
        IDispatcher dispatcher,
        IOptions<ChargilyOptions> options,
        ILogger<CompletePaymentCommand> logger,
        CancellationToken ct)
    {
        // 1. Read raw body as string
        httpContext.Request.EnableBuffering();
        using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8);
        var rawBody = await reader.ReadToEndAsync();

        // 2. Extract signature header
        if (!httpContext.Request.Headers.TryGetValue("signature", out var signatureHeader))
        {
            logger.LogWarning("Chargily webhook rejected: Missing signature header");
            return Results.BadRequest("Missing signature header");
        }

        // 3. Verify signature using Webhook Secret
        var chargilyOptions = options.Value;
        if (!VerifySignature(rawBody, chargilyOptions.WebhookSecret, signatureHeader.ToString()))
        {
            logger.LogWarning("Chargily webhook rejected: Invalid signature verification");
            return Results.BadRequest("Invalid signature");
        }

        // 4. Deserialize webhook payload
        WebhookEvent? ev;
        try
        {
            ev = JsonSerializer.Deserialize<WebhookEvent>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse webhook JSON body");
            return Results.BadRequest("Invalid JSON body");
        }

        if (ev?.Data is null || string.IsNullOrWhiteSpace(ev.Data.Id))
        {
            logger.LogWarning("Chargily webhook rejected: Missing data properties");
            return Results.BadRequest("Missing data properties");
        }

        // 4.5. Check environment-safety of webhook event (livemode check)
        var isLive = GetLivemodeValue(ev.Livemode);
        if (!isLive && !chargilyOptions.AllowTestMode)
        {
            logger.LogWarning("Chargily webhook rejected: Test mode event {EventId} is not allowed in this environment.", ev.Id);
            return Results.BadRequest("Test mode events are not allowed in this environment.");
        }

        logger.LogInformation("Received webhook event {EventId} of type {EventType} for checkout {CheckoutId}", ev.Id, ev.Type, ev.Data.Id);

        // 5. Send command to process payment if it is one of the status updates
        if (ev.Type.Equals("checkout.paid", StringComparison.OrdinalIgnoreCase) ||
            ev.Type.Equals("checkout.failed", StringComparison.OrdinalIgnoreCase) ||
            ev.Type.Equals("checkout.expired", StringComparison.OrdinalIgnoreCase))
        {
            var result = await dispatcher.SendAsync(
                new CompletePaymentCommand(ev.Data.Id, ev.Data.Status, ev.Data.Amount), ct);

            if (result.IsFailure)
            {
                logger.LogError("Error processing webhook payment completion: {Error}", result.Error.Message);
                return result.Error.ToProblem();
            }
        }

        return Results.Ok();
    }

    private static async Task<IResult> GetHistory(
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(
            new GetPaymentHistoryQuery(currentUser.UserId), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static bool VerifySignature(string rawBody, string secretKey, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(secretKey))
        {
            return false;
        }

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var payloadBytes = Encoding.UTF8.GetBytes(rawBody);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        var calculatedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(calculatedSignature),
            Encoding.UTF8.GetBytes(signatureHeader.ToLowerInvariant())
        );
    }

    private static bool GetLivemodeValue(JsonElement? livemodeElement)
    {
        if (livemodeElement == null)
        {
            return false;
        }

        var element = livemodeElement.Value;
        if (element.ValueKind == JsonValueKind.True)
        {
            return true;
        }
        if (element.ValueKind == JsonValueKind.False)
        {
            return false;
        }
        if (element.ValueKind == JsonValueKind.String)
        {
            var str = element.GetString();
            return bool.TryParse(str, out var result) && result;
        }
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetInt32(out var val) && val == 1;
        }

        return false;
    }

    // ── Webhook payload contracts ───────────────────────────────────────────
    private sealed record WebhookEvent(
        string Id,
        string Type,
        JsonElement? Livemode,
        WebhookData Data);

    private sealed record WebhookData(
        string Id,
        decimal Amount,
        string Status);
}
