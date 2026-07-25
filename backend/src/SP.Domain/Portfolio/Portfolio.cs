using SP.Domain.Abstractions;
using SP.Domain.Portfolio.Errors;
using SP.Domain.Shared;

namespace SP.Domain.Portfolio;

public sealed class Portfolio : AggregateRoot
{
    private readonly List<Project> _projects = [];

    private Portfolio() { }

    private Portfolio(Guid id, Guid providerId) : base(id)
    {
        ProviderId = providerId;
    }

    public Guid ProviderId { get; private set; }

    public IReadOnlyList<Project> Projects => _projects.AsReadOnly();

    public static Portfolio Create(Guid providerId)
    {
        return new Portfolio(Guid.NewGuid(), providerId);
    }

    public Result AddProject(string name, string description, List<Image> images)
    {
        var result = Project.Create(name, description, images);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        _projects.Add(result.Value);
        return Result.Success();
    }

    public Result RemoveProject(Guid projectId)
    {
        var project = _projects.FirstOrDefault(p => p.Id == projectId);
        if (project is null)
            return Result.Failure(PortfolioErrors.ProjectNotFound);

        _projects.Remove(project);
        return Result.Success();
    }
}