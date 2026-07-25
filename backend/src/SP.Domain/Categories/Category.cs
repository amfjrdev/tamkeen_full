using SP.Domain.Abstractions;
using SP.Domain.Categories.Errors;
using SP.Domain.Categories.Events;

namespace SP.Domain.Categories;

public sealed class Category : AggregateRoot
{
    private Category() { }

    private Category(Guid id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt= null;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Category Create(string name, string description)
    {
        var category = new Category(
            Guid.NewGuid(),
            name.Trim(),
            description?.Trim() ?? string.Empty);

        category.RaiseDomainEvent(new CategoryCreatedEvent(category.Id, category.Name));
        return category;
    }

    public Result Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(CategoryErrors.InvalidName);

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt= DateTime.UtcNow;
        RaiseDomainEvent(new CategoryUpdatedEvent(Id, Name));
        return Result.Success();
    }
}
