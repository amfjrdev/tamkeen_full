using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Queries.GetById;
using SP.Application.Categories.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Categories;
using SP.Domain.Categories.Errors;
using SP.Domain.Categories.Repositories;

namespace SP.Application.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : GetByIdQuery<CategoryResponseDto>(Id);

public sealed class GetCategoryByIdQueryHandler(ICategoryRepository repository)
    : GetByIdQueryHandler<Category, CategoryResponseDto>(repository),
      IQueryHandler<GetCategoryByIdQuery, CategoryResponseDto?>
{
    public Task<Result<CategoryResponseDto?>> HandleAsync(
        GetCategoryByIdQuery query, CancellationToken cancellationToken)
        => HandleAsync((GetByIdQuery<CategoryResponseDto>)query, cancellationToken);

    protected override Error NotFoundError() => CategoryErrors.NotFound;

    protected override CategoryResponseDto MapToResponse(Category entity) =>
        entity.ToResponse();
}