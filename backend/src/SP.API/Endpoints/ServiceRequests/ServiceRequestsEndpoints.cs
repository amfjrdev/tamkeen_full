using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.ServiceRequests.Commands.ApplyToServiceRequest;
using SP.Application.ServiceRequests.Commands.CompleteServiceRequest;
using SP.Application.ServiceRequests.Commands.CreateServiceRequest;
using SP.Application.ServiceRequests.Commands.ReviewServiceRequest;
using SP.Application.ServiceRequests.Commands.SelectProvider;
using SP.Application.ServiceRequests.Dtos;
using SP.Application.ServiceRequests.Queries.GetAvailableServiceRequests;
using SP.Application.ServiceRequests.Queries.GetClientServiceRequests;
using SP.Application.ServiceRequests.Queries.GetProviderApplications;
using SP.Application.ServiceRequests.Queries.GetServiceRequestDetails;
using SP.Domain.ServiceRequests.Enums;

namespace SP.API.Endpoints.ServiceRequests;

public static class ServiceRequestsEndpoints
{
    public static RouteGroupBuilder MapServiceRequestsEndpoints(this RouteGroupBuilder group)
    {
        // Client endpoints
        group.MapPost("/", Create).RequireAuthorization(AuthorizationPolicies.ClientOnly);
        group.MapGet("/my", GetMyRequests).RequireAuthorization(AuthorizationPolicies.ClientOnly);
        group.MapPost("/{id:guid}/select-provider", SelectProvider).RequireAuthorization(AuthorizationPolicies.ClientOnly);
        group.MapPost("/{id:guid}/complete", Complete).RequireAuthorization(AuthorizationPolicies.ClientOnly);
        group.MapPost("/{id:guid}/review", Review).RequireAuthorization(AuthorizationPolicies.ClientOnly);

        // Provider endpoints
        group.MapGet("/marketplace", GetMarketplaceRequests).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapPost("/{id:guid}/apply", Apply).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapGet("/provider-applications", GetProviderApplications).RequireAuthorization(AuthorizationPolicies.ProviderOnly);

        // Common details endpoint
        group.MapGet("/{id:guid}", GetById).RequireAuthorization();

        return group;
    }

    private static async Task<IResult> Create(
        [FromBody] CreateServiceRequestRequest body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new CreateServiceRequestCommand(
            currentUser.UserId,
            body.CategoryId,
            body.Title,
            body.Description,
            body.Wilaya,
            body.Budget);

        var result = await dispatcher.SendAsync(command, ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Created($"/api/service-requests/{result.Value}", new { id = result.Value });
    }

    private static async Task<IResult> GetMyRequests(
        [FromQuery] ServiceRequestStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ICurrentUser currentUser = null!,
        IDispatcher dispatcher = null!,
        CancellationToken ct = default)
    {
        var query = new GetClientServiceRequestsQuery(currentUser.UserId, status, page, pageSize);
        var result = await dispatcher.QueryAsync(query, ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetById(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var query = new GetServiceRequestDetailsQuery(id, currentUser.UserId);
        var result = await dispatcher.QueryAsync(query, ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.Ok(result.Value);
    }

    private static async Task<IResult> SelectProvider(
        Guid id,
        [FromBody] SelectProviderRequest body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new SelectProviderCommand(id, currentUser.UserId, body.ProviderId, body.ApplicationId);
        var result = await dispatcher.SendAsync(command, ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> Complete(
        Guid id,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new CompleteServiceRequestCommand(id, currentUser.UserId);
        var result = await dispatcher.SendAsync(command, ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> Review(
        Guid id,
        [FromBody] ReviewServiceRequestRequest body,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new ReviewServiceRequestCommand(id, currentUser.UserId, body.Rating, body.Comment);
        var result = await dispatcher.SendAsync(command, ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.NoContent();
    }

    private static async Task<IResult> GetMarketplaceRequests(
        [FromQuery] string? wilaya,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ICurrentUser currentUser = null!,
        IDispatcher dispatcher = null!,
        CancellationToken ct = default)
    {
        var query = new GetAvailableServiceRequestsQuery(currentUser.UserId, wilaya, categoryId, page, pageSize);
        var result = await dispatcher.QueryAsync(query, ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.Ok(result.Value);
    }

    private static async Task<IResult> Apply(
        Guid id,
        [FromBody] ApplyToServiceRequestRequest body,
        [FromQuery] string? wilaya,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new ApplyToServiceRequestCommand(
            id,
            currentUser.UserId,
            wilaya ?? string.Empty,
            body.CoverLetter,
            body.ProposedPrice);

        var result = await dispatcher.SendAsync(command, ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Created($"/api/service-requests/{id}/applications/{result.Value}", new { id = result.Value });
    }

    private static async Task<IResult> GetProviderApplications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ICurrentUser currentUser = null!,
        IDispatcher dispatcher = null!,
        CancellationToken ct = default)
    {
        var query = new GetProviderApplicationsQuery(currentUser.UserId, page, pageSize);
        var result = await dispatcher.QueryAsync(query, ct);
        return result.IsFailure ? result.Error.ToProblem() : Results.Ok(result.Value);
    }
}
