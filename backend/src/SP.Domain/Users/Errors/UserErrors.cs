using SP.Domain.Abstractions;

namespace SP.Domain.Users.Errors;

public static class UserErrors
{
    public static readonly Error NotFound =
        new("User.NotFound", "User not found.");

    public static readonly Error InvalidEmail =
        new("User.InvalidEmail", "Email address is invalid.");

    public static readonly Error EmailAlreadyInUse =
        new("User.EmailAlreadyInUse", "Email is already registered.");

    public static readonly Error InvalidFirstName =
        new("User.InvalidFirstName", "First name is required and must not exceed 50 characters.");

    public static readonly Error InvalidLastName =
        new("User.InvalidLastName", "Last name is required and must not exceed 50 characters.");

    public static readonly Error InvalidPhoneNumber =
        new("User.InvalidPhoneNumber", "Phone number format is invalid.");

    public static readonly Error InvalidPasswordHash =
        new("User.InvalidPasswordHash", "Password hash cannot be empty.");

    public static readonly Error CredentialNotFound =
        new("User.CredentialNotFound", "No credentials found for this user.");

    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "Invalid email or password.");

    public static readonly Error UserAlreadyVerified =
        new("User.AlreadyVerified", "Email address is already verified.");

    public static readonly Error AccountAlreadyDeleted =
        new("User.AccountAlreadyDeleted", "This account has been deleted.");

    public static readonly Error UserAlreadyBlocked =
        new("User.AlreadyBlocked", "This user is already blocked.");

    public static readonly Error UserNotBlocked =
        new("User.NotBlocked", "This user is not blocked.");

    public static readonly Error UserAlreadySuspended =
        new("User.AlreadySuspended", "This user is already suspended.");

    public static readonly Error UserNotSuspended =
        new("User.NotSuspended", "This user is not suspended.");

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Refresh token cannot be empty.");

    public static readonly Error RefreshTokenNotFound =
        new("User.RefreshTokenNotFound", "Refresh token not found.");

    public static readonly Error RefreshTokenNotActive =
        new("User.RefreshTokenNotActive", "Refresh token is expired or revoked.");

    public static readonly Error NoActiveRefreshTokens =
        new("User.NoActiveRefreshTokens", "No active refresh tokens found.");

    public static readonly Error InvalidPassword =
        new("User.InvalidPassword", "Invalid password.");

    public static readonly Error InvalidPasswordResetToken =
        new("User.InvalidPasswordResetToken", "Invalid Password ResetToken.");

    public static readonly Error InvalidConfirmationText =
        new("User.InvalidConfirmationText", "Invalid Confirmation Text.");

    public static readonly Error InvalidRole =
        new("User.InvalidRole", "Specified role is invalid.");

    public static readonly Error UserIsBlocked =
        new ("User.IsBlocked", "This user is blocked.");

    public static readonly Error UserIsSuspended =
        new("User.IsSuspended", "This user is Suspended.");
}