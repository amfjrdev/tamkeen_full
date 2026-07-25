// SP.Application/Portfolio/Commands/AddProject/AddProjectCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Portfolio.Errors;
using SP.Domain.Portfolio.Repositories;
using SP.Domain.Shared;

namespace SP.Application.Portfolio.Commands.AddProject;

public sealed record AddProjectCommand(
    Guid ProviderId,
    string Name,
    string Description,
    List<string> ImageUrls) : ICommand<Guid>;

public sealed class AddProjectCommandHandler : ICommandHandler<AddProjectCommand, Guid>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProjectCommandHandler(IPortfolioRepository portfolioRepository, IUnitOfWork unitOfWork)
    {
        _portfolioRepository = portfolioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(AddProjectCommand command, CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        var isNew = portfolio is null;

        if (isNew)
        {
            portfolio = SP.Domain.Portfolio.Portfolio.Create(command.ProviderId);
            await _portfolioRepository.AddAsync(portfolio, cancellationToken);
        }

        var images = command.ImageUrls
            .Select(url => Image.Create(url))
            .ToList();

        var result = portfolio!.AddProject(command.Name, command.Description, images);
        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var addedProject = portfolio.Projects.Last();

        // Only call Update() for existing portfolios.
        // For new ones the entity is already tracked as Added — calling Update()
        // flips it to Modified, drops the INSERT, and causes DbUpdateConcurrencyException.
        if (!isNew)
            _portfolioRepository.Update(portfolio);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(addedProject.Id);
    }
}
