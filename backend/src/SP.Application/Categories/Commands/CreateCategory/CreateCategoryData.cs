using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Commands.Create;
using SP.Domain.Abstractions;
using SP.Domain.Categories;
using SP.Domain.Categories.Errors;
using SP.Domain.Categories.Repositories;

namespace SP.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryData(string Name, string Description);

public sealed record CreateCategoryCommand(CreateCategoryData Entity)
    : CreateCommand<CreateCategoryData>(Entity);

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork)
    : CreateCommandHandler<Category, CreateCategoryData>(repository, unitOfWork),
      ICommandHandler<CreateCategoryCommand, Guid>
{
    public Task<Result<Guid>> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
        => HandleAsync((CreateCommand<CreateCategoryData>)command, cancellationToken);

    protected override async Task<Result> Validate(CreateCategoryData data)
    {
        if (await repository.ExistsByNameAsync(data.Name))
            return Result.Failure(CategoryErrors.DuplicateName);

        return Result.Success();
    }

    protected override Category CreateEntity(CreateCategoryData data) =>
        Category.Create(data.Name, data.Description);
}