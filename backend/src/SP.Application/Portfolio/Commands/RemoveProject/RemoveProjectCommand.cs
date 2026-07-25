// SP.Application/Portfolio/Commands/RemoveProject/RemoveProjectCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Portfolio.Errors;
using SP.Domain.Portfolio.Repositories;

namespace SP.Application.Portfolio.Commands.RemoveProject;

public sealed record RemoveProjectCommand(Guid ProviderId, Guid ProjectId) : ICommand;

public sealed class RemoveProjectCommandHandler : ICommandHandler<RemoveProjectCommand>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProjectCommandHandler(IPortfolioRepository portfolioRepository, IUnitOfWork unitOfWork)
    {
        _portfolioRepository = portfolioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(RemoveProjectCommand command, CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        if (portfolio is null)
            return Result.Failure(PortfolioErrors.ProjectNotFound);

        var result = portfolio.RemoveProject(command.ProjectId);
        if (result.IsFailure)
            return result;

        _portfolioRepository.Update(portfolio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
