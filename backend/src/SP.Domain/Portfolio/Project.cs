using SP.Domain.Abstractions;
using SP.Domain.Portfolio.Errors;
using SP.Domain.Shared;

namespace SP.Domain.Portfolio;

public sealed class Project : Entity
{
    private readonly List<Image> _projectImages = [];

    private Project() { }

    private Project(Guid id, string name, string description, List<Image> images) : base(id)
    {
        Name = name;
        Description = description;
        _projectImages = images;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public IReadOnlyList<Image> ProjectImages => _projectImages.AsReadOnly();

    public static Result<Project> Create(string name, string description, List<Image> images)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return Result.Failure<Project>(PortfolioErrors.InvalidProjectName);

        if (string.IsNullOrWhiteSpace(description) || description.Length > 1000)
            return Result.Failure<Project>(PortfolioErrors.InvalidProjectDescription);

        if (images is null || images.Count == 0)
            return Result.Failure<Project>(PortfolioErrors.ProjectMustHaveAtLeastOneImage);

        return Result.Success(new Project(Guid.NewGuid(), name.Trim(), description.Trim(), images));
    }

    public Result Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return Result.Failure(PortfolioErrors.InvalidProjectName);

        if (string.IsNullOrWhiteSpace(description) || description.Length > 1000)
            return Result.Failure(PortfolioErrors.InvalidProjectDescription);

        Name = name.Trim();
        Description = description.Trim();
        return Result.Success();
    }
}