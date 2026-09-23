using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using SP.Application.ServiceRequests.Commands.ApplyToServiceRequest;
using SP.Application.ServiceRequests.Commands.ApproveServiceRequest;
using SP.Application.ServiceRequests.Commands.CompleteServiceRequest;
using SP.Application.ServiceRequests.Commands.CreateServiceRequest;
using SP.Application.ServiceRequests.Commands.RejectServiceRequest;
using SP.Application.ServiceRequests.Commands.ReviewServiceRequest;
using SP.Application.ServiceRequests.Commands.SelectProvider;
using SP.Domain.Abstractions;
using SP.Domain.Categories;
using SP.Domain.Categories.Repositories;
using SP.Domain.Chat.Repositories;
using SP.Domain.Connects;
using SP.Domain.Connects.Repositories;
using SP.Domain.ServiceRequests;
using SP.Domain.ServiceRequests.Enums;
using SP.Domain.ServiceRequests.Repositories;
using Xunit;

namespace SP.UnitTests.Application.ServiceRequests;

public class ServiceRequestCommandHandlerTests
{
    private readonly IServiceRequestRepository _requestRepo = Substitute.For<IServiceRequestRepository>();
    private readonly ICategoryRepository _categoryRepo = Substitute.For<ICategoryRepository>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IConnectTransactionRepository _txRepo = Substitute.For<IConnectTransactionRepository>();
    private readonly IConversationRepository _convRepo = Substitute.For<IConversationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateServiceRequest_ValidCategory_ReturnsSuccess()
    {
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Plumbing", "Plumbing desc");
        _categoryRepo.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        var handler = new CreateServiceRequestCommandHandler(
            _requestRepo,
            _categoryRepo,
            _uow);

        var command = new CreateServiceRequestCommand(
            Guid.NewGuid(),
            categoryId,
            "Need a plumber in Constantine",
            "Water pipe burst in the kitchen",
            "Constantine",
            2000);

        var result = await handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _requestRepo.Received(1).AddAsync(Arg.Any<ServiceRequest>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveServiceRequest_PendingRequest_ApprovesSuccessfully()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", "Constantine").Value;

        _requestRepo.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);

        var handler = new ApproveServiceRequestCommandHandler(_requestRepo, _uow);
        var result = await handler.HandleAsync(new ApproveServiceRequestCommand(request.Id));

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Approved);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectServiceRequest_PendingRequest_RejectsSuccessfully()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", "Constantine").Value;

        _requestRepo.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);

        var handler = new RejectServiceRequestCommandHandler(_requestRepo, _uow);
        var result = await handler.HandleAsync(new RejectServiceRequestCommand(request.Id, "Inappropriate content"));

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Rejected);
        request.RejectionReason.Should().Be("Inappropriate content");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyToServiceRequest_SufficientConnects_DebitsWalletAndApplies()
    {
        var providerId = Guid.NewGuid();
        var request = ServiceRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", "Constantine").Value;
        request.Approve();

        var wallet = Wallet.Create(providerId, initialBalance: 50);

        _requestRepo.GetByIdWithApplicationsAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);
        _walletRepo.GetByUserIdAsync(providerId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var handler = new ApplyToServiceRequestCommandHandler(
            _requestRepo,
            _walletRepo,
            _txRepo,
            _convRepo,
            _uow);

        var command = new ApplyToServiceRequestCommand(
            request.Id,
            providerId,
            "Constantine",
            "I have 5 years experience.",
            1500,
            10);

        var result = await handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Should().Be(40); // 50 - 10
        request.Applications.Should().HaveCount(1);
        await _txRepo.Received(1).AddAsync(Arg.Any<ConnectTransaction>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyToServiceRequest_InsufficientConnects_FailsWithoutApplying()
    {
        var providerId = Guid.NewGuid();
        var request = ServiceRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", "Constantine").Value;
        request.Approve();

        var wallet = Wallet.Create(providerId, initialBalance: 5); // Needs 10

        _requestRepo.GetByIdWithApplicationsAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);
        _walletRepo.GetByUserIdAsync(providerId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var handler = new ApplyToServiceRequestCommandHandler(
            _requestRepo,
            _walletRepo,
            _txRepo,
            _convRepo,
            _uow);

        var command = new ApplyToServiceRequestCommand(
            request.Id,
            providerId,
            "Constantine",
            "I have 5 years experience.",
            1500,
            10);

        var result = await handler.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        wallet.Balance.Should().Be(5);
        request.Applications.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectProvider_AuthorizedClient_SelectsProvider()
    {
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var request = ServiceRequest.Create(
            clientId, Guid.NewGuid(), "Title", "Desc", "Constantine").Value;
        request.Approve();
        var app = request.AddApplication(providerId, "Constantine", "Letter", 1500, 10).Value;

        _requestRepo.GetByIdWithApplicationsAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(request);

        var handler = new SelectProviderCommandHandler(_requestRepo, _uow);
        var result = await handler.HandleAsync(new SelectProviderCommand(request.Id, clientId, providerId, app.Id));

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.ProviderSelected);
        request.SelectedProviderId.Should().Be(providerId);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
