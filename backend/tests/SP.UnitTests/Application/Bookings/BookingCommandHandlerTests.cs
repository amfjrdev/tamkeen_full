using FluentAssertions;
using NSubstitute;
using SP.Application.Bookings.Commands.AcceptBooking;
using SP.Application.Bookings.Commands.AddReport;
using SP.Application.Bookings.Commands.AddReview;
using SP.Application.Bookings.Commands.CancelBooking;
using SP.Application.Bookings.Commands.CompleteBooking;
using SP.Application.Bookings.Commands.CreateBooking;
using SP.Application.Bookings.Commands.RejectBooking;
using SP.Domain.Abstractions;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Repositories;
using SP.Domain.Services;
using SP.Domain.Services.Errors;
using SP.Domain.Services.Repositories;
using Xunit;

namespace SP.UnitTests.Application.Bookings;

public sealed class BookingCommandHandlerTests
{
    private readonly IBookingRepository _bookingRepo = Substitute.For<IBookingRepository>();
    private readonly IServiceRepository _serviceRepo = Substitute.For<IServiceRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid ProviderId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly DateTime FutureDate = DateTime.UtcNow.AddDays(3);

    // ── CreateBooking ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_WithActiveService_ReturnsBookingId()
    {
        var service = CreateActiveService();
        _serviceRepo.GetByIdAsync(ServiceId).Returns(service);
        _uow.SaveChangesAsync().Returns(1);

        var handler = new CreateBookingCommandHandler(_bookingRepo, _serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateBookingCommand(ClientId, ServiceId, FutureDate));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _bookingRepo.Received(1).AddAsync(Arg.Any<Booking>());
    }

    [Fact]
    public async Task CreateBooking_WithNonExistentService_ReturnsFailure()
    {
        _serviceRepo.GetByIdAsync(ServiceId).Returns((Service?)null);

        var handler = new CreateBookingCommandHandler(_bookingRepo, _serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateBookingCommand(ClientId, ServiceId, FutureDate));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.ServiceNotFound);
    }

    [Fact]
    public async Task CreateBooking_WithInactiveService_ReturnsFailure()
    {
        var service = CreateActiveService();
        service.Deactivate();
        _serviceRepo.GetByIdAsync(ServiceId).Returns(service);

        var handler = new CreateBookingCommandHandler(_bookingRepo, _serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateBookingCommand(ClientId, ServiceId, FutureDate));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.ServiceUnavailable);
    }

    [Fact]
    public async Task CreateBooking_WithPastDate_ReturnsFailure()
    {
        var service = CreateActiveService();
        _serviceRepo.GetByIdAsync(ServiceId).Returns(service);

        var handler = new CreateBookingCommandHandler(_bookingRepo, _serviceRepo, _uow);
        var result = await handler.HandleAsync(
            new CreateBookingCommand(ClientId, ServiceId, DateTime.UtcNow.AddDays(-1)));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.InvalidScheduledDate);
    }

    // ── AcceptBooking ─────────────────────────────────────────────────────

    [Fact]
    public async Task AcceptBooking_ByCorrectProvider_ReturnsSuccess()
    {
        var booking = CreatePendingBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new AcceptBookingCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new AcceptBookingCommand(booking.Id, ProviderId));

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Accepted);
    }

    [Fact]
    public async Task AcceptBooking_ByWrongProvider_ReturnsFailure()
    {
        var booking = CreatePendingBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new AcceptBookingCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new AcceptBookingCommand(booking.Id, Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.UnauthorizedAction);
    }

    [Fact]
    public async Task AcceptBooking_WhenNotFound_ReturnsFailure()
    {
        _bookingRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Booking?)null);

        var handler = new AcceptBookingCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new AcceptBookingCommand(Guid.NewGuid(), ProviderId));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.NotFound);
    }

    // ── RejectBooking ─────────────────────────────────────────────────────

    [Fact]
    public async Task RejectBooking_ByCorrectProvider_ReturnsSuccess()
    {
        var booking = CreatePendingBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new RejectBookingCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new RejectBookingCommand(booking.Id, ProviderId));

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Rejected);
    }

    // ── CancelBooking ─────────────────────────────────────────────────────

    [Fact]
    public async Task CancelBooking_ByCorrectClient_ReturnsSuccess()
    {
        var booking = CreatePendingBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new CancelBookingCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new CancelBookingCommand(booking.Id, ClientId));

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task CancelBooking_ByWrongClient_ReturnsFailure()
    {
        var booking = CreatePendingBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new CancelBookingCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new CancelBookingCommand(booking.Id, Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.UnauthorizedAction);
    }

    // ── CompleteBooking ───────────────────────────────────────────────────

    [Fact]
    public async Task CompleteBooking_WhenAccepted_ReturnsSuccess()
    {
        var booking = CreateAcceptedBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new CompleteBookingCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new CompleteBookingCommand(booking.Id, ProviderId));

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Completed);
    }

    // ── AddReview ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddReview_WhenCompleted_ReturnsSuccess()
    {
        var booking = CreateCompletedBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new AddReviewCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new AddReviewCommand(booking.Id, ClientId, 5, "Excellent!"));

        result.IsSuccess.Should().BeTrue();
        booking.Review.Should().NotBeNull();
    }

    [Fact]
    public async Task AddReview_ByWrongClient_ReturnsFailure()
    {
        var booking = CreateCompletedBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new AddReviewCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new AddReviewCommand(booking.Id, Guid.NewGuid(), 5, "Excellent!"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.UnauthorizedAction);
    }

    // ── AddReport ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddReport_WhenCompleted_ReturnsSuccess()
    {
        var booking = CreateCompletedBooking();
        _bookingRepo.GetByIdAsync(booking.Id).Returns(booking);

        var handler = new AddReportCommandHandler(_bookingRepo, _uow);
        var result = await handler.HandleAsync(
            new AddReportCommand(booking.Id, ClientId, "Provider was rude"));

        result.IsSuccess.Should().BeTrue();
        booking.Report.Should().NotBeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Service CreateActiveService()
        => Service.Create(ProviderId, CategoryId, "Haircut", "desc", 25m, 30).Value;

    private static Booking CreatePendingBooking()
        => Booking.Create(ClientId, ProviderId, ServiceId, FutureDate).Value;

    private static Booking CreateAcceptedBooking()
    {
        var b = CreatePendingBooking();
        b.Accept();
        return b;
    }

    private static Booking CreateCompletedBooking()
    {
        var b = CreateAcceptedBooking();
        b.Complete();
        return b;
    }
}
