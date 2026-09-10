using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Commands.Delete;
using SP.Domain.Abstractions;
using SP.Domain.Categories;
using SP.Domain.Categories.Errors;
using SP.Domain.Categories.Repositories;

namespace SP.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : DeleteCommand(Id);

public sealed class DeleteCategoryCommandHandler(ICategoryRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Result.Failure(CategoryErrors.NotFound);

        await repository.HardDeleteAsync(command.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
