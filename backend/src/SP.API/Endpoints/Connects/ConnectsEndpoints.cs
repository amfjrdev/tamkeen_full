using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Connects.Queries.GetConnectBalance;
using SP.Application.Connects.Queries.GetConnectPacks;
using SP.Application.Connects.Commands.PurchaseConnects;
using Microsoft.Extensions.Options;
using SP.Application.Payments.Commands.CreateCheckoutSession;
using SP.Infrastructure.Payments.Chargily;

namespace SP.API.Endpoints.Connects;

public sealed record PurchaseConnectsRequest(string PackId, string IdempotencyKey);
public sealed record CheckoutConnectsRequest(string PackId);

public static class ConnectsEndpoints
{
    public static RouteGroupBuilder MapConnectsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/balance", GetBalance).RequireAuthorization();
        group.MapGet("/packs", GetPacks).RequireAuthorization();
        group.MapPost("/purchase", Purchase).RequireAuthorization();
        group.MapPost("/checkout", Checkout).RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetBalance(
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(
            new GetConnectBalanceQuery(currentUser.UserId), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetPacks(
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(
            new GetConnectPacksQuery(), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> Purchase(
        [FromBody] PurchaseConnectsRequest request,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new PurchaseConnectsCommand(currentUser.UserId, request.PackId, request.IdempotencyKey), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> Checkout(
        [FromBody] CheckoutConnectsRequest request,
        HttpContext httpContext,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        IOptions<ChargilyOptions> options,
        CancellationToken ct)
    {
        var chargilyOptions = options.Value;
        var successUrl = chargilyOptions.SuccessUrl;
        var failureUrl = chargilyOptions.FailureUrl;

        if (string.IsNullOrWhiteSpace(successUrl) || string.IsNullOrWhiteSpace(failureUrl))
        {
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            if (string.IsNullOrWhiteSpace(successUrl)) successUrl = $"{baseUrl}/payments/success";
            if (string.IsNullOrWhiteSpace(failureUrl)) failureUrl = $"{baseUrl}/payments/failure";
        }

        var result = await dispatcher.SendAsync(
            new CreateCheckoutSessionCommand(currentUser.UserId, request.PackId, successUrl, failureUrl), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(new { checkoutUrl = result.Value.CheckoutUrl, checkoutId = result.Value.CheckoutId });
    }
}
