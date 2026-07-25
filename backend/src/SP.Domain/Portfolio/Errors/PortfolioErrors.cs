using SP.Domain.Abstractions;

namespace SP.Domain.Portfolio.Errors;

public static class PortfolioErrors
{
    public static readonly Error NotFound = new
        ("Portfolio.NotFound", "Portfolio not found.");
    public static readonly Error ProjectNotFound = new
        ("Portfolio.ProjectNotFound", "Project not found in the portfolio.");
    public static Error InvalidProjectName = new
        ("Portfolio.InvalidProjectName", "Project name is required and must not exceed 100 characters.");
    public static Error InvalidProjectDescription = new
        ("Portfolio.InvalidProjectDescription", "Project Description is required and must not exceed 100 characters.");
    public static Error ProjectMustHaveAtLeastOneImage = new
        ("Portfolio.ProjectMustHaveAtLeastOneImage", "Project Must Have At Least One Image.");
}