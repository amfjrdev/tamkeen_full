using SP.Domain.Abstractions;

namespace SP.Domain.Categories.Errors;

public static class CategoryErrors
{
    public static readonly Error InvalidName = new
        ("Category.InvalidCategoryName", "Category name cannot be empty or exceed 100 characters.");

    public static readonly Error NotFound = new
        ("Category.NotFound", "Category not found.");

    public static Error DuplicateName = new
        ("Category.DuplicateName", "Category Name is  Duplicate.");
}