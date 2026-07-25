using SP.Domain.Abstractions;

namespace SP.Domain.Bookings.Errors;

public static class BookingErrors
{
    public static readonly Error NotFound = new("Booking.NotFound", "Booking not found.");
    public static readonly Error NotPending = new("Booking.NotPending", "Booking is not in pending status.");
    public static readonly Error NotAccepted = new("Booking.NotAccepted", "Booking is not in accepted status.");
    public static readonly Error NotCompleted = new("Booking.NotCompleted", "Booking is not in completed status.");
    public static readonly Error ChatNotAvailable = new("Booking.ChatNotAvailable", "Chat is not available for this booking.");
    public static readonly Error AlreadyReviewed = new("Booking.AlreadyReviewed", "Booking has already been reviewed.");
    public static readonly Error AlreadyReported = new("Booking.AlreadyReported", "Booking has already been reported.");
    public static readonly Error InvalidRating = new("Booking.InvalidRating", "Rating must be between 1 and 5.");

    // Kept for backward compatibility
    public static readonly Error InvalidClientId = new("Booking.InvalidClientId", "Client ID cannot be empty.");
    public static readonly Error InvalidProviderId = new("Booking.InvalidProviderId", "Provider ID cannot be empty.");
    public static readonly Error InvalidServiceId = new("Booking.InvalidServiceId", "Service ID cannot be empty.");
    public static readonly Error InvalidScheduledDate = new("Booking.InvalidScheduledDate", "Scheduled date must be in the future.");
    public static readonly Error BookingAlreadyConfirmed = new("Booking.AlreadyConfirmed", "Booking is already confirmed.");
    public static readonly Error BookingAlreadyCancelled = new("Booking.AlreadyCancelled", "Booking is already cancelled.");
    public static readonly Error BookingAlreadyCompleted = new("Booking.AlreadyCompleted", "Booking is already completed.");
    public static readonly Error CannotCancelConfirmedBooking = new("Booking.CannotCancelConfirmed", "Cannot cancel a confirmed booking.");
    public static readonly Error CannotCompleteUnconfirmedBooking = new("Booking.CannotCompleteUnconfirmed", "Cannot complete an unconfirmed booking.");
    public static readonly Error UnauthorizedAction = new("Booking.Unauthorized", "You are not authorized to perform this action on this booking.");
    public static readonly Error CannotExpireNonPendingBooking = new("Booking.CannotExpireNonPending", "Cannot expire a non-pending booking.");
    public static readonly Error CannotAcceptNonPendingBooking = new("Booking.CannotAcceptNonPending", "Cannot accept a non-pending booking.");
    public static readonly Error CannotRejectNonPendingBooking = new("Booking.CannotRejectNonPending", "Cannot reject a non-pending booking.");
    public static readonly Error CannotConfirmNonAcceptedBooking = new("Booking.CannotConfirmNonAccepted", "Cannot confirm a non-accepted booking.");
    public static readonly Error BookingAlreadyRejected = new("Booking.AlreadyRejected", "Booking is already rejected.");
}
