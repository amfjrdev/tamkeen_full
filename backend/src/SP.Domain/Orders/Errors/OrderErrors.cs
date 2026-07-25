using SP.Domain.Abstractions;

namespace SP.Domain.Orders.Errors;

public static class OrderErrors
{
    public static readonly Error PaymentNotFound =
        new("Order.PaymentNotFound",
            "No payment associated with this order.");
}