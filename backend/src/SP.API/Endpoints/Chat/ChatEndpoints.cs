using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Chat.Queries.GetConversations;
using SP.Application.Chat.Queries.GetMessages;
using SP.Application.Chat.Queries.GetConversationDetails;
using SP.Application.Chat.Commands.UnlockConversation;
using SP.Application.Chat.Commands.CreateConversation;
using SP.Application.Chat.Commands.SendMessage;

namespace SP.API.Endpoints.Chat;

public static class ChatEndpoints
{
    public static RouteGroupBuilder MapChatEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/conversations", GetConversations).RequireAuthorization();
        group.MapGet("/conversations/{id:guid}", GetConversationDetails).RequireAuthorization();
        group.MapPost("/conversations", CreateConversationHandler).RequireAuthorization();
        group.MapGet("/messages/{conversationId:guid}", GetMessages).RequireAuthorization();
        group.MapPost("/conversations/{id:guid}/unlock", UnlockConversation).RequireAuthorization();
        group.MapPost("/messages", SendMessageHandler).RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetConversations(
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(
            new GetConversationsQuery(currentUser.UserId), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetConversationDetails(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(
            new GetConversationDetailsQuery(currentUser.UserId, id), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateConversationHandler(
        [FromBody] CreateConversationBody body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new CreateConversationCommand(currentUser.UserId, body.ProviderId), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetMessages(
        Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ICurrentUser currentUser = null!,
        IDispatcher dispatcher = null!,
        CancellationToken ct = default)
    {
        var result = await dispatcher.QueryAsync(
            new GetMessagesQuery(currentUser.UserId, conversationId, page, pageSize), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> SendMessageHandler(
        [FromBody] SendMessageBody request,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new SendMessageCommand(currentUser.UserId, request.ConversationId, request.Text), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> UnlockConversation(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new UnlockConversationCommand(currentUser.UserId, id), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private sealed record CreateConversationBody(Guid ProviderId);
    private sealed record SendMessageBody(Guid ConversationId, string Text);
}