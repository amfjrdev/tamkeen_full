using FluentAssertions;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Errors;
using Xunit;

namespace SP.UnitTests.Domain;

public sealed class BookingAggregateTests
{
    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid ProviderId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly DateTime FutureDate = DateTime.UtcNow.AddDays(3);

    // ── Factory ───────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsPendingBooking()
    {
        var result = Booking.Create(ClientId, ProviderId, ServiceId, FutureDate);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(BookingStatus.Pending);
        result.Value.ClientId.Should().Be(ClientId);
        result.Value.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithPastDate_ReturnsFailure()
    {
        var result = Booking.Create(ClientId, ProviderId, ServiceId, DateTime.UtcNow.AddDays(-1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.InvalidScheduledDate);
    }

    [Fact]
    public void Create_WithEmptyClientId_ReturnsFailure()
    {
        var result = Booking.Create(Guid.Empty, ProviderId, ServiceId, FutureDate);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.InvalidClientId);
    }

    [Fact]
    public void Create_WithEmptyProviderId_ReturnsFailure()
    {
        var result = Booking.Create(ClientId, Guid.Empty, ServiceId, FutureDate);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.InvalidProviderId);
    }

    // ── Accept ────────────────────────────────────────────────────────────

    [Fact]
    public void Accept_WhenPending_CreatesChat()
    {
        var booking = CreatePendingBooking();

        var result = booking.Accept();

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Accepted);
        booking.ChatRoom.Should().NotBeNull();
        booking.DomainEvents.Should().HaveCount(2); // Created + Accepted
    }

    [Fact]
    public void Accept_WhenAlreadyAccepted_ReturnsFailure()
    {
        var booking = CreatePendingBooking();
        booking.Accept();

        var result = booking.Accept();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.NotPending);
    }

    [Fact]
    public void Accept_WhenCancelled_ReturnsFailure()
    {
        var booking = CreatePendingBooking();
        booking.Cancel();

        var result = booking.Accept();

        result.IsFailure.Should().BeTrue();
    }

    // ── Reject ────────────────────────────────────────────────────────────

    [Fact]
    public void Reject_WhenPending_ReturnsSuccess()
    {
        var booking = CreatePendingBooking();

        var result = booking.Reject();

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Rejected);
    }

    [Fact]
    public void Reject_WhenAccepted_ReturnsFailure()
    {
        var booking = CreatePendingBooking();
        booking.Accept();

        var result = booking.Reject();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.NotPending);
    }

    // ── Complete ──────────────────────────────────────────────────────────

    [Fact]
    public void Complete_WhenAccepted_ReturnsSuccess()
    {
        var booking = CreatePendingBooking();
        booking.Accept();

        var result = booking.Complete();

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Completed);
        booking.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenPending_ReturnsFailure()
    {
        var booking = CreatePendingBooking();

        var result = booking.Complete();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.NotAccepted);
    }

    // ── Cancel ────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenPending_ReturnsSuccess()
    {
        var booking = CreatePendingBooking();

        var result = booking.Cancel();

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ReturnsFailure()
    {
        var booking = CreatePendingBooking();
        booking.Cancel();

        var result = booking.Cancel();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.BookingAlreadyCancelled);
    }

    [Fact]
    public void Cancel_WhenCompleted_ReturnsFailure()
    {
        var booking = CreatePendingBooking();
        booking.Accept();
        booking.Complete();

        var result = booking.Cancel();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.BookingAlreadyCompleted);
    }

    // ── Review ────────────────────────────────────────────────────────────

    [Fact]
    public void AddReview_WhenCompleted_ReturnsSuccess()
    {
        var booking = CreateCompletedBooking();

        var result = booking.AddReview(ClientId, 5, "Excellent!");

        result.IsSuccess.Should().BeTrue();
        booking.Review.Should().NotBeNull();
        booking.Review!.Rating.Should().Be(5);
    }

    [Fact]
    public void AddReview_WhenNotCompleted_ReturnsFailure()
    {
        var booking = CreatePendingBooking();

        var result = booking.AddReview(ClientId, 5, "Great");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.NotCompleted);
    }

    [Fact]
    public void AddReview_WhenAlreadyReviewed_ReturnsFailure()
    {
        var booking = CreateCompletedBooking();
        booking.AddReview(ClientId, 4, "Good");

        var result = booking.AddReview(ClientId, 5, "Excellent");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.AlreadyReviewed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void AddReview_WithInvalidRating_ReturnsFailure(int rating)
    {
        var booking = CreateCompletedBooking();

        var result = booking.AddReview(ClientId, rating, "comment");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.InvalidRating);
    }

    // ── Report ────────────────────────────────────────────────────────────

    [Fact]
    public void AddReport_WhenCompleted_ReturnsSuccess()
    {
        var booking = CreateCompletedBooking();

        var result = booking.AddReport(ClientId, "Provider was rude");

        result.IsSuccess.Should().BeTrue();
        booking.Report.Should().NotBeNull();
    }

    [Fact]
    public void AddReport_WhenAlreadyReported_ReturnsFailure()
    {
        var booking = CreateCompletedBooking();
        booking.AddReport(ClientId, "First report");

        var result = booking.AddReport(ClientId, "Second report");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.AlreadyReported);
    }

    // ── Chat ──────────────────────────────────────────────────────────────

    [Fact]
    public void SendMessage_WhenAccepted_ReturnsMessage()
    {
        var booking = CreatePendingBooking();
        booking.Accept();

        var result = booking.SendMessage(ClientId, "Hello!");

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Hello!");
        result.Value.SenderId.Should().Be(ClientId);
    }

    [Fact]
    public void SendMessage_WhenPending_ReturnsFailure()
    {
        var booking = CreatePendingBooking();

        var result = booking.SendMessage(ClientId, "Hello!");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookingErrors.ChatNotAvailable);
    }

    // ── Expiry ────────────────────────────────────────────────────────────

    [Fact]
    public void Expire_WhenPending_ReturnsSuccess()
    {
        var booking = CreatePendingBooking();

        var result = booking.Expire();

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Expired);
    }

    [Fact]
    public void IsExpired_WhenOlderThanTimeout_ReturnsTrue()
    {
        var booking = CreatePendingBooking();

        // Simulate old booking by checking with zero timeout
        var isExpired = booking.IsExpired(TimeSpan.Zero);

        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenAccepted_ReturnsFalse()
    {
        var booking = CreatePendingBooking();
        booking.Accept();

        var isExpired = booking.IsExpired(TimeSpan.Zero);

        isExpired.Should().BeFalse();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Booking CreatePendingBooking()
        => Booking.Create(ClientId, ProviderId, ServiceId, FutureDate).Value;

    private static Booking CreateCompletedBooking()
    {
        var booking = CreatePendingBooking();
        booking.Accept();
        booking.Complete();
        return booking;
    }
}
