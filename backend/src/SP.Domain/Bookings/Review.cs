using SP.Domain.Abstractions;
using SP.Domain.Bookings.Errors;

namespace SP.Domain.Bookings;

public sealed class Review : Entity
{
    private Review() { }

    private Review(Guid id, Guid clientId, Guid serviceId, Guid bookingId, int rating, string comment) : base(id)
    {
        ClientId = clientId;
        ServiceId = serviceId;
        BookingId = bookingId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ClientId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid BookingId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    internal static Result<Review> Create(Guid clientId, Guid serviceId, Guid bookingId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            return Result.Failure<Review>(BookingErrors.InvalidRating);

        return Result.Success(new Review(Guid.NewGuid(), clientId, serviceId, bookingId, rating, comment?.Trim() ?? string.Empty));
    }
}
