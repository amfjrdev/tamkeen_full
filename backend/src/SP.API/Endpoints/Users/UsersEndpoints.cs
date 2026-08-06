using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Files;
using SP.Application.Abstractions.Messaging;
using SP.Application.Users.Commands.BlockUser;
using SP.Application.Users.Commands.DeleteUserAccount;
using SP.Application.Users.Commands.SuspendUser;
using SP.Application.Users.Commands.UnblockUser;
using SP.Application.Users.Commands.UnsuspendUser;
using SP.Application.Users.Commands.UpdateUserPhoneNumber;
using SP.Application.Users.Commands.UpdateUserProfile;
using SP.Application.Users.Commands.UpdateUserProfilePicture;
using SP.Application.Users.Commands.UpdateUserLocation;
using SP.Application.Users.Queries.GetAllUsers;
using SP.Application.Users.Queries.GetUserById;
using SP.Application.Users.Queries.GetMyProfile;
using SP.Application.Users.Queries.GetProviders;

namespace SP.API.Endpoints.Users;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetMe).RequireAuthorization();
        group.MapGet("/providers", GetProviders).AllowAnonymous();
        group.MapGet("/{id:guid}", GetById).RequireAuthorization();
        group.MapGet("/", GetAll).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPut("/profile", UpdateProfile).RequireAuthorization();
        group.MapPost("/profile-picture", UpdateProfilePicture).RequireAuthorization().DisableAntiforgery();
        group.MapPost("/upload", UploadMedia).RequireAuthorization().DisableAntiforgery();
        group.MapPatch("/phone", UpdatePhone).RequireAuthorization();
        group.MapPatch("/location", UpdateLocation).RequireAuthorization();
        group.MapDelete("/", DeleteAccount).RequireAuthorization();

        // Admin actions
        group.MapPost("/{id:guid}/block", Block).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPost("/{id:guid}/unblock", Unblock).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPost("/{id:guid}/suspend", Suspend).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPost("/{id:guid}/unsuspend", Unsuspend).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return group;
    }

    private static async Task<IResult> GetMe(
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetMyProfileQuery(currentUser.UserId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetProviders(
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetProvidersQuery(), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetUserByIdQuery(id), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetAll(
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetAllUsersQuery(), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateProfile(
        [FromBody] UpdateProfileBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new UpdateUserProfileCommand(currentUser.UserId, body.FirstName, body.LastName, body.PhoneNumber), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> UpdateProfilePicture(
        IFormFile file,
        ICurrentUser currentUser,
        IFileService fileService,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest("No file uploaded.");
        }

        var urls = await fileService.UploadFilesAsync(new List<IFormFile> { file }, "profile-pictures", ct);
        if (urls.Count == 0)
        {
            return Results.BadRequest("Failed to upload profile picture.");
        }

        var pictureUrl = urls[0];

        var result = await dispatcher.SendAsync(
            new UpdateUserProfilePictureCommand(currentUser.UserId, pictureUrl), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(new { url = pictureUrl });
    }

    private static async Task<IResult> UploadMedia(
        IFormFile file,
        IFileService fileService,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest("No file uploaded.");
        }

        var urls = await fileService.UploadFilesAsync(new List<IFormFile> { file }, "chat-media", ct);
        if (urls.Count == 0)
        {
            return Results.BadRequest("Failed to upload file.");
        }

        return Results.Ok(new { url = urls[0] });
    }

    private static async Task<IResult> UpdatePhone(
        [FromBody] UpdatePhoneBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new UpdateUserPhoneNumberCommand(currentUser.UserId, body.PhoneNumber), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> DeleteAccount(
        [FromBody] DeleteAccountBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new DeleteUserAccountCommand(currentUser.UserId, body.ConfirmationText, body.Password), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> UpdateLocation(
        [FromBody] UpdateLocationBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new UpdateUserLocationCommand(currentUser.UserId, body.Latitude, body.Longitude), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> Block(
        Guid id,
        [FromBody] ReasonBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new BlockUserCommand(currentUser.UserId, id, body.Reason), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> Unblock(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new UnblockUserCommand(id), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> Suspend(
        Guid id,
        [FromBody] ReasonBody body,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new SuspendUserCommand(id, body.Reason), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> Unsuspend(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new UnsuspendUserCommand(id), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    // ── Request bodies ────────────────────────────────────────────────────
    private sealed record UpdateProfileBody(string FirstName, string LastName, string? PhoneNumber);
    private sealed record UpdatePhoneBody(string? PhoneNumber);
    private sealed record UpdateLocationBody(double Latitude, double Longitude);
    private sealed record DeleteAccountBody(string ConfirmationText, string Password);
    private sealed record ReasonBody(string? Reason);
}
