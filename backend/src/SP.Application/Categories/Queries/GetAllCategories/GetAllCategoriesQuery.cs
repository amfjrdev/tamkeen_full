using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Commands.Create;
using SP.Application.Categories.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Categories;
using SP.Domain.Categories.Repositories;

namespace SP.Application.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesQuery : GetAllQuery<CategoryResponseDto>;

public sealed class GetAllCategoriesQueryHandler(ICategoryRepository repository)
    : GetAllQueryHandler<Category, CategoryResponseDto>(repository),
      IQueryHandler<GetAllCategoriesQuery, IReadOnlyList<CategoryResponseDto>>
{
    public Task<Result<IReadOnlyList<CategoryResponseDto>>> HandleAsync(
        GetAllCategoriesQuery query, CancellationToken cancellationToken)
        => HandleAsync((GetAllQuery<CategoryResponseDto>)query, cancellationToken);

    protected override CategoryResponseDto MapToResponse(Category entity) =>
        entity.ToResponse();
}