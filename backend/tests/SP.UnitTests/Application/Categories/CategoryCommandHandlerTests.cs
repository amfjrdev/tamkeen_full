using FluentAssertions;
using NSubstitute;
using SP.Application.Abstractions.Messaging.Commands.Create;
using SP.Application.Abstractions.Messaging.Commands.Delete;
using SP.Application.Abstractions.Messaging.Commands.Update;
using SP.Application.Categories.Commands.CreateCategory;
using SP.Application.Categories.Commands.DeleteCategory;
using SP.Application.Categories.Commands.UpdateCategory;
using SP.Domain.Abstractions;
using SP.Domain.Categories;
using SP.Domain.Categories.Errors;
using SP.Domain.Categories.Repositories;
using Xunit;

namespace SP.UnitTests.Application.Categories;

public sealed class CategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepo = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    // ── CreateCategory ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCategory_WhenNameIsUnique_ReturnsId()
    {
        _categoryRepo.ExistsByNameAsync("Plumbing").Returns(false);
        _uow.SaveChangesAsync().Returns(1);

        var handler = new CreateCategoryCommandHandler(_categoryRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateCategoryCommand(new CreateCategoryData("Plumbing", "Plumbing services")));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateCategory_WhenNameIsDuplicate_ReturnsFailure()
    {
        _categoryRepo.ExistsByNameAsync("Plumbing").Returns(true);

        var handler = new CreateCategoryCommandHandler(_categoryRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateCategoryCommand(new CreateCategoryData("Plumbing", "desc")));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.DuplicateName);
    }

    // ── UpdateCategory ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateCategory_WhenExists_ReturnsSuccess()
    {
        var category = Category.Create("Plumbing", "desc");
        _categoryRepo.GetByIdAsync(category.Id).Returns(category);
        _categoryRepo.ExistsByNameAsync("Electrical").Returns(false);

        var handler = new UpdateCategoryCommandHandler(_categoryRepo, _uow);
        var result = await handler.HandleAsync(
            new UpdateCategoryCommand(category.Id, new UpdateCategoryData("Electrical", "Electrical services")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Electrical");
    }

    [Fact]
    public async Task UpdateCategory_WhenNotFound_ReturnsFailure()
    {
        _categoryRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Category?)null);

        var handler = new UpdateCategoryCommandHandler(_categoryRepo, _uow);
        var result = await handler.HandleAsync(
            new UpdateCategoryCommand(Guid.NewGuid(), new UpdateCategoryData("Name", "desc")),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.NotFound);
    }

    // ── DeleteCategory ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteCategory_WhenExists_ReturnsSuccess()
    {
        var category = Category.Create("Plumbing", "desc");
        _categoryRepo.GetByIdAsync(category.Id).Returns(category);

        var handler = new DeleteCategoryCommandHandler(_categoryRepo, _uow);
        var result = await handler.HandleAsync(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _categoryRepo.Received(1).HardDeleteAsync(category.Id);
    }

    [Fact]
    public async Task DeleteCategory_WhenNotFound_ReturnsFailure()
    {
        _categoryRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Category?)null);

        var handler = new DeleteCategoryCommandHandler(_categoryRepo, _uow);
        var result = await handler.HandleAsync(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.NotFound);
    }
}
