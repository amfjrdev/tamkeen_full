using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.API.Mappings;
using SP.Application.Abstractions.Messaging;
using SP.Application.Users.Commands;
using SP.Application.Users.Commands.Login;
using SP.Application.Users.Commands.Logout;
using SP.Application.Users.Commands.RefreshToken;
using SP.Application.Users.Commands.Register;
using SP.Application.Users.Commands.RequestPasswordReset;
using SP.Application.Users.Commands.ResetPassword;

namespace SP.API.Endpoints.Authentication;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", Register).AllowAnonymous();
        group.MapPost("/login", Login).AllowAnonymous();
        group.MapPost("/refresh", Refresh).AllowAnonymous();
        group.MapPost("/logout", Logout).RequireAuthorization();
        group.MapPost("/verify-email", VerifyEmail).RequireAuthorization();
        group.MapPost("/request-password-reset", RequestPasswordReset).AllowAnonymous();
        group.MapPost("/reset-password", ResetPassword).AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterCommand command,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(command, ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Created("/api/users", result.Value!.ToDto());
    }

    private static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(command, ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value!.ToDto());
    }

    private static async Task<IResult> Refresh(
        [FromBody] RefreshTokenRequestBody body,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new RefreshTokenCommand(body.RefreshToken), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value!.ToDto());
    }

    private static async Task<IResult> Logout(
        [FromBody] LogoutRequestBody body,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new LogoutCommand(body.RefreshToken), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> VerifyEmail(
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new VerifyUserEmailCommand(currentUser.UserId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> RequestPasswordReset(
        [FromBody] PasswordResetRequestBody body,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        // Always returns 204 — never reveals whether email exists
        await dispatcher.SendAsync(new RequestPasswordResetCommand(body.Email), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordBody body,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new ResetPasswordCommand(body.Token, body.NewPassword), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    // ── Request bodies ────────────────────────────────────────────────────
    private sealed record RefreshTokenRequestBody(string RefreshToken);
    private sealed record LogoutRequestBody(string RefreshToken);
    private sealed record PasswordResetRequestBody(string Email);
    private sealed record ResetPasswordBody(string Token, string NewPassword);
}
