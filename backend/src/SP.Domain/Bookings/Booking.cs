using SP.Domain.Abstractions;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Events;

namespace SP.Domain.Bookings;

public sealed class Booking : AggregateRoot
{
    private Review? _review;
    private Report? _report;
    private ChatRoom? _chatRoom;

    private Booking() { }

    private Booking(Guid id, Guid clientId, Guid providerId, Guid serviceId, DateTime scheduledDate)
        : base(id)
    {
        ClientId = clientId;
        ProviderId = providerId;
        ServiceId = serviceId;
        ScheduledDate = scheduledDate;
        Status = BookingStatus.Pending;
        RequestedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ClientId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Review? Review => _review;
    public Report? Report => _report;
    public ChatRoom? ChatRoom => _chatRoom;

    public static Result<Booking> Create(Guid clientId, Guid providerId, Guid serviceId, DateTime scheduledDate)
    {
        if (clientId == Guid.Empty)
            return Result.Failure<Booking>(BookingErrors.InvalidClientId);

        if (providerId == Guid.Empty)
            return Result.Failure<Booking>(BookingErrors.InvalidProviderId);

        if (serviceId == Guid.Empty)
            return Result.Failure<Booking>(BookingErrors.InvalidServiceId);

        if (scheduledDate <= DateTime.UtcNow)
            return Result.Failure<Booking>(BookingErrors.InvalidScheduledDate);

        var booking = new Booking(Guid.NewGuid(), clientId, providerId, serviceId, scheduledDate);
        booking.RaiseDomainEvent(new BookingRequestedEvent(booking.Id, booking.ClientId, booking.ServiceId));
        return Result.Success(booking);
    }

    public Result Accept()
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure(BookingErrors.NotPending);

        Status = BookingStatus.Accepted;
        ScheduledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _chatRoom = ChatRoom.Create(Id);
        RaiseDomainEvent(new BookingAcceptedEvent(Id, ClientId, ServiceId));
        return Result.Success();
    }

    public Result Reject()
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure(BookingErrors.NotPending);

        Status = BookingStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new BookingRejectedEvent(Id, ClientId));
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != BookingStatus.Accepted)
            return Result.Failure(BookingErrors.NotAccepted);

        Status = BookingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new BookingCompletedEvent(Id, ClientId, ServiceId));
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            return Result.Failure(BookingErrors.BookingAlreadyCancelled);

        if (Status == BookingStatus.Completed)
            return Result.Failure(BookingErrors.BookingAlreadyCompleted);

        if (Status == BookingStatus.Rejected)
            return Result.Failure(BookingErrors.BookingAlreadyRejected);

        Status = BookingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Expire()
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure(BookingErrors.CannotExpireNonPendingBooking);

        Status = BookingStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result<ChatMessage> SendMessage(Guid senderId, string content)
    {
        if (Status != BookingStatus.Accepted || _chatRoom is null)
            return Result.Failure<ChatMessage>(BookingErrors.ChatNotAvailable);

        var message = _chatRoom.AddMessage(senderId, content);
        RaiseDomainEvent(new MessageSentEvent(Id, message.Id, senderId));
        return Result.Success(message);
    }

    public Result AddReview(Guid clientId, int rating, string comment)
    {
        if (Status != BookingStatus.Completed)
            return Result.Failure(BookingErrors.NotCompleted);

        if (_review is not null)
            return Result.Failure(BookingErrors.AlreadyReviewed);

        var result = Review.Create(clientId, ServiceId, Id, rating, comment);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        _review = result.Value;
        RaiseDomainEvent(new BookingReviewedEvent(Id, ServiceId, rating));
        return Result.Success();
    }

    public Result AddReport(Guid reporterId, string reason)
    {
        if (Status != BookingStatus.Completed)
            return Result.Failure(BookingErrors.NotCompleted);

        if (_report is not null)
            return Result.Failure(BookingErrors.AlreadyReported);

        _report = Report.Create(Id, reporterId, reason);
        RaiseDomainEvent(new BookingReportedEvent(Id, _report.Id, reporterId));
        return Result.Success();
    }

    public Result ResolveReport()
    {
        if (_report is null)
            return Result.Failure(new Error("Booking.NoReport", "This booking has no report to resolve."));

        _report.Resolve();
        return Result.Success();
    }

    public bool IsExpired(TimeSpan expirationTimeout)
        => Status == BookingStatus.Pending && DateTime.UtcNow - CreatedAt > expirationTimeout;
}
