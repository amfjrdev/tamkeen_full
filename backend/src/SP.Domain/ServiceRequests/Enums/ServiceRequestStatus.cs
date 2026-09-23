namespace SP.Domain.ServiceRequests.Enums;

public enum ServiceRequestStatus
{
    PendingAdminReview = 1,
    Approved = 2,
    Rejected = 3,
    ProviderSelected = 4,
    Completed = 5,
    Cancelled = 6
}
