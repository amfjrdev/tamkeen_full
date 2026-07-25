using SP.Domain.Abstractions;
using SP.Domain.Users.Errors;
using SP.Domain.Users.Events;
using System.Text.RegularExpressions;
using SP.Domain.Shared;

namespace SP.Domain.Users;

public sealed class User : AggregateRoot
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+?[\d\s\-\(\)]{10,15}$", RegexOptions.Compiled);

    private UserCredential? _credential;
    private readonly List<RefreshToken> _refreshTokens = [];

    private User() { }

    private User(Guid id, string email, string firstName, string lastName, string? phoneNumber, UserRole role)
        : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Role = role;
        UserProfilePicture = Image.Default;
        IsEmailVerified = false;
        IsDeleted = false;
        IsBlocked = false;
        IsSuspended = false;
        CreatedAt = DateTime.UtcNow;
    }

    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? BlockReason { get; private set; }
    public string? SuspendReason { get; private set; }
    public UserRole Role { get; private set; }
    public Image UserProfilePicture { get; set; } = Image.Default;
    public bool IsEmailVerified { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsBlocked { get; private set; }
    public bool IsSuspended { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime? BlockedAt { get; private set; }
    public DateTime? SuspendedAt { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public DateTime? LastLocationUpdate { get; private set; }

    public UserCredential? Credential => _credential;
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // ── Factory ────────────────────────────────────────────────────────────

    public static Result<User> Create(
        string email,
        string firstName,
        string lastName,
        string? phoneNumber,
        UserRole role )
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            return Result.Failure<User>(UserErrors.InvalidEmail);

        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 50)
            return Result.Failure<User>(UserErrors.InvalidFirstName);

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > 50)
            return Result.Failure<User>(UserErrors.InvalidLastName);

        if (!string.IsNullOrEmpty(phoneNumber) && !PhoneRegex.IsMatch(phoneNumber))
            return Result.Failure<User>(UserErrors.InvalidPhoneNumber);

        var user = new User(
            Guid.NewGuid(),
            email.ToLowerInvariant().Trim(),
            firstName.Trim(),
            lastName.Trim(),
            phoneNumber?.Trim(),
            role);

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email, user.Role));
        return Result.Success(user);
    }

    // ── Credential ─────────────────────────────────────────────────────────

    public Result SetCredential(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure(UserErrors.InvalidPasswordHash);

        _credential = UserCredential.Create(Id, passwordHash);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            return Result.Failure(UserErrors.InvalidPasswordHash);

        if (_credential is null)
            return Result.Failure(UserErrors.CredentialNotFound);

        _credential.UpdatePassword(newPasswordHash);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // ── Refresh tokens ─────────────────────────────────────────────────────

    public Result AddRefreshToken(string token, int expirationDays = 7)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure(UserErrors.InvalidRefreshToken);

        foreach (var existing in _refreshTokens.Where(t => t.IsActive))
            existing.Revoke();

        _refreshTokens.Add(RefreshToken.Create(Id, token, expirationDays));
        return Result.Success();
    }

    public Result RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);

        if (refreshToken is null)
            return Result.Failure(UserErrors.RefreshTokenNotFound);

        if (!refreshToken.IsActive)
            return Result.Failure(UserErrors.RefreshTokenNotActive);

        refreshToken.Revoke();
        return Result.Success();
    }

    public Result RevokeAllRefreshTokens()
    {
        var activeTokens = _refreshTokens.Where(t => t.IsActive).ToList();

        if (!activeTokens.Any())
            return Result.Failure(UserErrors.NoActiveRefreshTokens);

        foreach (var token in activeTokens)
            token.Revoke();

        return Result.Success();
    }

    // ── Profile ────────────────────────────────────────────────────────────

    public Result UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 50)
            return Result.Failure(UserErrors.InvalidFirstName);

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > 50)
            return Result.Failure(UserErrors.InvalidLastName);

        if (!string.IsNullOrEmpty(phoneNumber) && !PhoneRegex.IsMatch(phoneNumber))
            return Result.Failure(UserErrors.InvalidPhoneNumber);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdatePhoneNumber(string? phoneNumber)
    {
        if (!string.IsNullOrEmpty(phoneNumber) && !PhoneRegex.IsMatch(phoneNumber))
            return Result.Failure(UserErrors.InvalidPhoneNumber);

        PhoneNumber = phoneNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // ── Email verification ─────────────────────────────────────────────────

    public Result VerifyEmail()
    {
        if (IsEmailVerified)
            return Result.Failure(UserErrors.UserAlreadyVerified);

        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserEmailVerifiedEvent(Id, Email));
        return Result.Success();
    }

    // ── Account status ─────────────────────────────────────────────────────

    public Result Block(string? reason = null)
    {
        if (IsDeleted)
            return Result.Failure(UserErrors.AccountAlreadyDeleted);

        if (IsBlocked)
            return Result.Failure(UserErrors.UserAlreadyBlocked);

        IsBlocked = true;
        BlockReason = reason;
        BlockedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserBlockedEvent(Id, reason));
        return Result.Success();
    }

    public Result Unblock()
    {
        if (!IsBlocked)
            return Result.Failure(UserErrors.UserNotBlocked);

        IsBlocked = false;
        BlockReason = null;
        BlockedAt = null;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserUnblockedEvent(Id));
        return Result.Success();
    }

    public Result Suspend(string? reason = null)
    {
        if (IsDeleted)
            return Result.Failure(UserErrors.AccountAlreadyDeleted);

        if (IsSuspended)
            return Result.Failure(UserErrors.UserAlreadySuspended);

        IsSuspended = true;
        SuspendReason = reason;
        SuspendedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserSuspendedEvent(Id, reason));
        return Result.Success();
    }

    public Result Unsuspend()
    {
        if (!IsSuspended)
            return Result.Failure(UserErrors.UserNotSuspended);

        IsSuspended = false;
        SuspendReason = null;
        SuspendedAt = null;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserUnsuspendedEvent(Id));
        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
            return Result.Failure(UserErrors.AccountAlreadyDeleted);

        IsDeleted = true;
        IsBlocked = false;
        IsSuspended = false;
        BlockReason = null;
        SuspendReason = null;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        foreach (var token in _refreshTokens.Where(t => t.IsActive))
            token.Revoke();

        RaiseDomainEvent(new UserDeletedEvent(Id));
        return Result.Success();
    }

    //-----------------

    public void UpdateProfilePicture(Image newProfilePicture)
    {
        if (newProfilePicture is null)
            throw new ArgumentNullException(nameof(newProfilePicture));

        UserProfilePicture = newProfilePicture;
        RaiseDomainEvent(new UserProfilePictureUpdatedDomainEvent(Id, newProfilePicture.Url));
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        LastLocationUpdate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}