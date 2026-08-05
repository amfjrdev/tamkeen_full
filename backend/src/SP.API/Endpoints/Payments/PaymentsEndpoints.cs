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
        group.MapGet("/success", ServeSuccessPage);
        group.MapGet("/failure", ServeFailurePage);

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

    private static IResult ServeSuccessPage()
    {
        return Results.Content(SuccessHtml, "text/html", Encoding.UTF8);
    }

    private static IResult ServeFailurePage()
    {
        return Results.Content(FailureHtml, "text/html", Encoding.UTF8);
    }

    private static readonly string SuccessHtml = """
        <!DOCTYPE html>
        <html lang="fr">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Paiement Réussi - Tamkeen</title>
            <style>
                body {
                    background-color: #0f172a;
                    color: #ffffff;
                    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    height: 100vh;
                    margin: 0;
                }
                .card {
                    background: rgba(30, 41, 59, 0.7);
                    backdrop-filter: blur(10px);
                    border: 1px solid rgba(255, 255, 255, 0.1);
                    padding: 40px;
                    border-radius: 24px;
                    text-align: center;
                    max-width: 400px;
                    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
                    animation: fadeIn 0.6s ease-out;
                }
                .icon-circle {
                    width: 80px;
                    height: 80px;
                    background: rgba(16, 185, 129, 0.1);
                    border: 2px solid #10b981;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 24px;
                }
                .icon-circle svg {
                    color: #10b981;
                    width: 40px;
                    height: 40px;
                }
                h1 {
                    font-size: 24px;
                    margin: 0 0 12px;
                    font-weight: 700;
                }
                p {
                    color: #94a3b8;
                    font-size: 15px;
                    line-height: 1.5;
                    margin: 0 0 24px;
                }
                @keyframes fadeIn {
                    from { opacity: 0; transform: translateY(20px); }
                    to { opacity: 1; transform: translateY(0); }
                }
            </style>
        </head>
        <body>
            <div class="card">
                <div class="icon-circle">
                    <svg fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5"></path>
                    </svg>
                </div>
                <h1>Paiement Réussi !</h1>
                <p>Votre transaction a été validée avec succès. Vos crédits ont été ajoutés à votre portefeuille.</p>
                <p>Vous pouvez maintenant fermer cette fenêtre en toute sécurité et retourner sur l'application Tamkeen.</p>
            </div>
        </body>
        </html>
        """;

    private static readonly string FailureHtml = """
        <!DOCTYPE html>
        <html lang="fr">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Échec du Paiement - Tamkeen</title>
            <style>
                body {
                    background-color: #0f172a;
                    color: #ffffff;
                    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    height: 100vh;
                    margin: 0;
                }
                .card {
                    background: rgba(30, 41, 59, 0.7);
                    backdrop-filter: blur(10px);
                    border: 1px solid rgba(255, 255, 255, 0.1);
                    padding: 40px;
                    border-radius: 24px;
                    text-align: center;
                    max-width: 400px;
                    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
                    animation: fadeIn 0.6s ease-out;
                }
                .icon-circle {
                    width: 80px;
                    height: 80px;
                    background: rgba(239, 68, 68, 0.1);
                    border: 2px solid #ef4444;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 24px;
                }
                .icon-circle svg {
                    color: #ef4444;
                    width: 40px;
                    height: 40px;
                }
                h1 {
                    font-size: 24px;
                    margin: 0 0 12px;
                    font-weight: 700;
                }
                p {
                    color: #94a3b8;
                    font-size: 15px;
                    line-height: 1.5;
                    margin: 0 0 24px;
                }
                @keyframes fadeIn {
                    from { opacity: 0; transform: translateY(20px); }
                    to { opacity: 1; transform: translateY(0); }
                }
            </style>
        </head>
        <body>
            <div class="card">
                <div class="icon-circle">
                    <svg fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </div>
                <h1>Échec du Paiement</h1>
                <p>Votre transaction n'a pas pu être validée ou a été annulée. Aucun montant n'a été débité.</p>
                <p>Vous pouvez fermer cette fenêtre en toute sécurité et retourner sur l'application Tamkeen pour réessayer.</p>
            </div>
        </body>
        </html>
        """;
}

