using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Commands.Update;
using SP.Domain.Abstractions;
using SP.Domain.Categories;
using SP.Domain.Categories.Errors;
using SP.Domain.Categories.Repositories;

namespace SP.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryData(string Name, string Description);

public sealed record UpdateCategoryCommand(Guid Id, UpdateCategoryData Entity)
    : UpdateCommand<UpdateCategoryData>(Id, Entity);

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork)
    : UpdateCommandHandler<Category, UpdateCategoryData>(repository, unitOfWork),
      ICommandHandler<UpdateCategoryCommand>
{
    public Task<Result> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
        => HandleAsync((UpdateCommand<UpdateCategoryData>)command, cancellationToken);

    protected override Error NotFoundError() => CategoryErrors.NotFound;

    protected override async Task<Result> Validate(UpdateCategoryData data, Category entity)
    {
        if (entity.Name != data.Name && await repository.ExistsByNameAsync(data.Name))
            return Result.Failure(CategoryErrors.DuplicateName);

        return Result.Success();
    }

    protected override Result UpdateEntity(Category entity, UpdateCategoryData data) =>
        entity.Update(data.Name, data.Description);
}