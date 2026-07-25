using SP.Domain.Categories;

namespace SP.Application.Categories.Dtos;

public sealed record CategoryResponseDto(
    Guid id,
    string name,
    string description,
    string iconName,
    string iconColor,
    string backgroundColor,
    int displayOrder,
    DateTime createdAt,
    DateTime? updatedAt
    );

public static class UserMapper
{
    public static CategoryResponseDto ToResponse(this Category category, CategoryMetadata? metadata = null)
    {
        return new CategoryResponseDto(
            category.Id,
            category.Name,
            category.Description,
            metadata?.IconName ?? "default",
            metadata?.IconColor ?? "#6B7280",
            metadata?.BackgroundColor ?? "#F3F4F6",
            metadata?.DisplayOrder ?? 0,
            category.CreatedAt,
            category.UpdatedAt
        );
    }
}

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Description,
    string IconName,
    string IconColor,
    string BackgroundColor,
    int DisplayOrder
);