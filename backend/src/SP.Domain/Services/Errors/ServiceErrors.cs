using SP.Domain.Abstractions;

namespace SP.Domain.Services.Errors;

public static class ServiceErrors
{
    public static readonly Error InvalidServiceName = new("Service.InvalidName", "Service name cannot be empty or exceed 100 characters.");
    public static readonly Error InvalidServiceDescription = new("Service.InvalidDescription", "Service description cannot exceed 500 characters.");
    public static readonly Error InvalidPrice = new("Service.InvalidPrice", "Service price must be greater than zero.");
    public static readonly Error InvalidDuration = new("Service.InvalidDuration", "Service duration must be greater than zero minutes.");
    public static readonly Error InvalidProviderId = new("Service.InvalidProviderId", "Provider ID cannot be empty.");
    public static readonly Error ServiceNotFound = new("Service.NotFound", "Service not found.");
    public static readonly Error ServiceAlreadyActive = new("Service.AlreadyActive", "Service is already active.");
    public static readonly Error ServiceAlreadyInactive = new("Service.AlreadyInactive", "Service is already inactive.");
    public static readonly Error ServiceUnavailable = new("Service.Unavailable", "Service is currently unavailable.");
    public static readonly Error CannotDeleteActiveService = new("Service.CannotDeleteActive", "Cannot delete an active service. Deactivate it first.");
    public static readonly Error InvalidCategoryId = new("Service.InvalidCategoryId", "Category ID cannot be empty.");
}