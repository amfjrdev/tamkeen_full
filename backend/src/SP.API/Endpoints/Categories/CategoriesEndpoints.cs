using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Categories.Commands.CreateCategory;
using SP.Application.Categories.Commands.DeleteCategory;
using SP.Application.Categories.Commands.UpdateCategory;
using SP.Application.Categories.Queries.GetAllCategories;
using SP.Application.Categories.Queries.GetCategoryById;

namespace SP.API.Endpoints.Categories;

public static class CategoriesEndpoints
{
    public static RouteGroupBuilder MapCategoriesEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", Create).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPut("/{id:guid}", Update).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapDelete("/{id:guid}", Delete).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/{id:guid}", GetById).AllowAnonymous();
        group.MapGet("/", GetAll).AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Create(
        [FromBody] CreateCategoryData data,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new CreateCategoryCommand(data), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Created($"/api/categories/{result.Value}", new { id = result.Value });
    }

    private static async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateCategoryData data,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new UpdateCategoryCommand(id, data), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> Delete(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new DeleteCategoryCommand(id), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetCategoryByIdQuery(id), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetAll(
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetAllCategoriesQuery(), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }
}
