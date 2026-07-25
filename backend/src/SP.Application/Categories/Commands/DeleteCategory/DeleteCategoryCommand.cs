using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Commands.Delete;
using SP.Domain.Abstractions;
using SP.Domain.Categories;
using SP.Domain.Categories.Errors;
using SP.Domain.Categories.Repositories;

namespace SP.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : DeleteCommand(Id);

public sealed class DeleteCategoryCommandHandler(ICategoryRepository repository, IUnitOfWork unitOfWork)
    : DeleteCommandHandler<Category>(repository, unitOfWork),
      ICommandHandler<DeleteCategoryCommand>
{
    public Task<Result> HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
        => HandleAsync((DeleteCommand)command, cancellationToken);

    protected override Error NotFoundError() => CategoryErrors.NotFound;
}
