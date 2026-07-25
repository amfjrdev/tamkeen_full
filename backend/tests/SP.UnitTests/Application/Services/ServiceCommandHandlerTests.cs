using FluentAssertions;
using NSubstitute;
using SP.Application.Services.Commands.ActivateService;
using SP.Application.Services.Commands.CreateService;
using SP.Application.Services.Commands.DeactivateService;
using SP.Application.Services.Commands.UpdateService;
using SP.Domain.Abstractions;
using SP.Domain.Services;
using SP.Domain.Services.Errors;
using SP.Domain.Services.Repositories;
using Xunit;

namespace SP.UnitTests.Application.Services;

public sealed class ServiceCommandHandlerTests
{
    private readonly IServiceRepository _serviceRepo = Substitute.For<IServiceRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private static readonly Guid ProviderId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    // ── CreateService ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateService_WhenNameIsUnique_ReturnsServiceId()
    {
        _serviceRepo.ExistsForProviderAsync(ProviderId, "Haircut").Returns(false);
        _uow.SaveChangesAsync().Returns(1);

        var handler = new CreateServiceCommandHandler(_serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateServiceCommand(ProviderId, CategoryId, "Haircut", "desc", 25m, 30));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _serviceRepo.Received(1).AddAsync(Arg.Any<Service>());
    }

    [Fact]
    public async Task CreateService_WhenNameAlreadyExists_ReturnsFailure()
    {
        _serviceRepo.ExistsForProviderAsync(ProviderId, "Haircut").Returns(true);

        var handler = new CreateServiceCommandHandler(_serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateServiceCommand(ProviderId, CategoryId, "Haircut", "desc", 25m, 30));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidServiceName);
    }

    [Fact]
    public async Task CreateService_WithInvalidPrice_ReturnsFailure()
    {
        _serviceRepo.ExistsForProviderAsync(ProviderId, "Haircut").Returns(false);

        var handler = new CreateServiceCommandHandler(_serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateServiceCommand(ProviderId, CategoryId, "Haircut", "desc", -1m, 30));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidPrice);
    }

    // ── UpdateService ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateService_ByCorrectProvider_ReturnsSuccess()
    {
        var service = CreateService();
        _serviceRepo.GetByIdAsync(service.Id).Returns(service);

        var handler = new UpdateServiceCommandHandler(_serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new UpdateServiceCommand(service.Id, ProviderId, "New Name", "new desc", 50m, 60));

        result.IsSuccess.Should().BeTrue();
        service.Price.Should().Be(50m);
    }

    [Fact]
    public async Task UpdateService_ByWrongProvider_ReturnsFailure()
    {
        var service = CreateService();
        _serviceRepo.GetByIdAsync(service.Id).Returns(service);

        var handler = new UpdateServiceCommandHandler(_serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new UpdateServiceCommand(service.Id, Guid.NewGuid(), "Name", "desc", 50m, 60));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidProviderId);
    }

    [Fact]
    public async Task UpdateService_WhenNotFound_ReturnsFailure()
    {
        _serviceRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Service?)null);

        var handler = new UpdateServiceCommandHandler(_serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new UpdateServiceCommand(Guid.NewGuid(), ProviderId, "Name", "desc", 50m, 60));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.ServiceNotFound);
    }

    // ── ActivateService ───────────────────────────────────────────────────

    [Fact]
    public async Task ActivateService_WhenInactive_ReturnsSuccess()
    {
        var service = CreateService();
        service.Deactivate();
        _serviceRepo.GetByIdAsync(service.Id).Returns(service);

        var handler = new ActivateServiceCommandHandler(_serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new ActivateServiceCommand(service.Id, ProviderId));

        result.IsSuccess.Should().BeTrue();
        service.IsActive.Should().BeTrue();
    }

    // ── DeactivateService ─────────────────────────────────────────────────

    [Fact]
    public async Task DeactivateService_WhenActive_ReturnsSuccess()
    {
        var service = CreateService();
        _serviceRepo.GetByIdAsync(service.Id).Returns(service);

        var handler = new DeactivateServiceCommandHandler(_serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new DeactivateServiceCommand(service.Id, ProviderId));

        result.IsSuccess.Should().BeTrue();
        service.IsActive.Should().BeFalse();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Service CreateService()
        => Service.Create(ProviderId, CategoryId, "Haircut", "desc", 25m, 30).Value;
}
