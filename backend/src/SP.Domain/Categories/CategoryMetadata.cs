namespace SP.Domain.Categories;

public sealed class CategoryMetadata
{
    public Guid CategoryId { get; private set; }
    public string IconName { get; private set; } = string.Empty;
    public string IconColor { get; private set; } = string.Empty;
    public string BackgroundColor { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private CategoryMetadata() { }

    public static CategoryMetadata Create(
        Guid categoryId,
        string iconName,
        string iconColor,
        string backgroundColor,
        int displayOrder)
    {
        return new CategoryMetadata
        {
            CategoryId = categoryId,
            IconName = iconName,
            IconColor = iconColor,
            BackgroundColor = backgroundColor,
            DisplayOrder = displayOrder
        };
    }

    public void Update(string iconName, string iconColor, string backgroundColor, int displayOrder)
    {
        IconName = iconName;
        IconColor = iconColor;
        BackgroundColor = backgroundColor;
        DisplayOrder = displayOrder;
    }
}
