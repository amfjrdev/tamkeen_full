using SP.Domain.Abstractions;

namespace SP.Domain.ServiceRequests.Errors;

public static class ServiceRequestErrors
{
    public static readonly Error NotFound = new(
        "ServiceRequest.NotFound",
        "The requested service request was not found.");

    public static readonly Error InvalidTitle = new(
        "ServiceRequest.InvalidTitle",
        "Title is required and must not exceed 120 characters.");

    public static readonly Error InvalidDescription = new(
        "ServiceRequest.InvalidDescription",
        "Description is required and must not exceed 2000 characters.");

    public static readonly Error InvalidWilaya = new(
        "ServiceRequest.InvalidWilaya",
        "Wilaya is required.");

    public static readonly Error InvalidBudget = new(
        "ServiceRequest.InvalidBudget",
        "Budget cannot be negative.");

    public static readonly Error InvalidCategory = new(
        "ServiceRequest.InvalidCategory",
        "Valid category is required.");

    public static readonly Error NotPendingReview = new(
        "ServiceRequest.NotPendingReview",
        "The request is not in pending review status.");

    public static readonly Error NotApproved = new(
        "ServiceRequest.NotApproved",
        "Applications can only be submitted to approved requests.");

    public static readonly Error WilayaMismatch = new(
        "ServiceRequest.WilayaMismatch",
        "Provider Wilaya does not match the request location.");

    public static readonly Error AlreadyApplied = new(
        "ServiceRequest.AlreadyApplied",
        "You have already applied to this service request.");

    public static readonly Error ProviderAlreadySelected = new(
        "ServiceRequest.ProviderAlreadySelected",
        "A provider has already been selected for this request.");

    public static readonly Error ApplicationNotFound = new(
        "ServiceRequest.ApplicationNotFound",
        "The specified application was not found.");

    public static readonly Error NotInProviderSelectedState = new(
        "ServiceRequest.NotInProviderSelectedState",
        "The request is not in an active provider state to be completed.");

    public static readonly Error NotCompleted = new(
        "ServiceRequest.NotCompleted",
        "Only completed requests can be reviewed.");

    public static readonly Error AlreadyReviewed = new(
        "ServiceRequest.AlreadyReviewed",
        "This service request has already been reviewed.");

    public static readonly Error UnauthorizedAction = new(
        "ServiceRequest.UnauthorizedAction",
        "You are not authorized to perform this action.");

    public static readonly Error InsufficientConnects = new(
        "ServiceRequest.InsufficientConnects",
        "Insufficient connects balance to apply for this request.");

    public static readonly Error InvalidRating = new(
        "ServiceRequest.InvalidRating",
        "Rating must be an integer between 1 and 5.");
}
