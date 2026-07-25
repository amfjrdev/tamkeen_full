using SP.Domain.Abstractions;

namespace SP.Domain.Payments.Errors;

public static class PaymentErrors
{
    public static readonly Error InvalidStatusForPaid =
        new("Payment.InvalidStatusForPaid",
            "Cannot mark payment as paid from the current status.");

    public static readonly Error InvalidStatusForFailed =
        new("Payment.InvalidStatusForFailed",
            "Cannot mark payment as failed from the current status.");

    public static readonly Error NotFound =
        new("Payment.NotFound",
            "The specified payment transaction was not found.");
}
