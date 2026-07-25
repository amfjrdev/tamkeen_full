using FluentAssertions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;
using Xunit;

namespace SP.UnitTests.Domain;

public sealed class UserAggregateTests
{
    // ── Factory ───────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = User.Create("john@example.com", "John", "Doe", null, UserRole.Client);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("john@example.com");
        result.Value.FirstName.Should().Be("John");
        result.Value.Role.Should().Be(UserRole.Client);
        result.Value.IsEmailVerified.Should().BeFalse();
        result.Value.IsDeleted.Should().BeFalse();
        result.Value.DomainEvents.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("no@")]
    public void Create_WithInvalidEmail_ReturnsFailure(string email)
    {
        var result = User.Create(email, "John", "Doe", null, UserRole.Client);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_NormalizesEmailToLowercase()
    {
        var result = User.Create("JOHN@EXAMPLE.COM", "John", "Doe", null, UserRole.Client);

        result.Value.Email.Should().Be("john@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyFirstName_ReturnsFailure(string firstName)
    {
        var result = User.Create("john@example.com", firstName, "Doe", null, UserRole.Client);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidFirstName);
    }

    [Fact]
    public void Create_WithFirstNameExceeding50Chars_ReturnsFailure()
    {
        var result = User.Create("john@example.com", new string('A', 51), "Doe", null, UserRole.Client);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidFirstName);
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("0501234567")]
    [InlineData("+44 20 7946 0958")]
    public void Create_WithValidPhoneNumber_ReturnsSuccess(string phone)
    {
        var result = User.Create("john@example.com", "John", "Doe", phone, UserRole.Client);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithInvalidPhoneNumber_ReturnsFailure()
    {
        var result = User.Create("john@example.com", "John", "Doe", "abc", UserRole.Client);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidPhoneNumber);
    }

    // ── Credential ────────────────────────────────────────────────────────

    [Fact]
    public void SetCredential_WithValidHash_ReturnsSuccess()
    {
        var user = CreateValidUser();

        var result = user.SetCredential("$2a$11$hashedpassword");

        result.IsSuccess.Should().BeTrue();
        user.Credential.Should().NotBeNull();
    }

    [Fact]
    public void SetCredential_WithEmptyHash_ReturnsFailure()
    {
        var user = CreateValidUser();

        var result = user.SetCredential("");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidPasswordHash);
    }

    // ── Refresh tokens ────────────────────────────────────────────────────

    [Fact]
    public void AddRefreshToken_RevokesExistingActiveTokens()
    {
        var user = CreateValidUser();
        user.AddRefreshToken("token-1");

        user.AddRefreshToken("token-2");

        user.RefreshTokens.Where(t => t.Token == "token-1").Single().IsActive.Should().BeFalse();
        user.RefreshTokens.Where(t => t.Token == "token-2").Single().IsActive.Should().BeTrue();
    }

    [Fact]
    public void RevokeRefreshToken_WithActiveToken_ReturnsSuccess()
    {
        var user = CreateValidUser();
        user.AddRefreshToken("my-token");

        var result = user.RevokeRefreshToken("my-token");

        result.IsSuccess.Should().BeTrue();
        user.RefreshTokens.Single().IsActive.Should().BeFalse();
    }

    [Fact]
    public void RevokeRefreshToken_WithNonExistentToken_ReturnsFailure()
    {
        var user = CreateValidUser();

        var result = user.RevokeRefreshToken("ghost-token");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.RefreshTokenNotFound);
    }

    // ── Email verification ────────────────────────────────────────────────

    [Fact]
    public void VerifyEmail_WhenNotVerified_ReturnsSuccess()
    {
        var user = CreateValidUser();

        var result = user.VerifyEmail();

        result.IsSuccess.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_ReturnsFailure()
    {
        var user = CreateValidUser();
        user.VerifyEmail();

        var result = user.VerifyEmail();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UserAlreadyVerified);
    }

    // ── Block / Unblock ───────────────────────────────────────────────────

    [Fact]
    public void Block_WhenNotBlocked_ReturnsSuccess()
    {
        var user = CreateValidUser();

        var result = user.Block("spam");

        result.IsSuccess.Should().BeTrue();
        user.IsBlocked.Should().BeTrue();
        user.BlockReason.Should().Be("spam");
    }

    [Fact]
    public void Block_WhenAlreadyBlocked_ReturnsFailure()
    {
        var user = CreateValidUser();
        user.Block();

        var result = user.Block();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UserAlreadyBlocked);
    }

    [Fact]
    public void Unblock_WhenBlocked_ClearsBlockReason()
    {
        var user = CreateValidUser();
        user.Block("reason");

        var result = user.Unblock();

        result.IsSuccess.Should().BeTrue();
        user.IsBlocked.Should().BeFalse();
        user.BlockReason.Should().BeNull();
    }

    [Fact]
    public void Unblock_WhenNotBlocked_ReturnsFailure()
    {
        var user = CreateValidUser();

        var result = user.Unblock();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UserNotBlocked);
    }

    // ── Suspend / Unsuspend ───────────────────────────────────────────────

    [Fact]
    public void Suspend_WhenNotSuspended_ReturnsSuccess()
    {
        var user = CreateValidUser();

        var result = user.Suspend("violation");

        result.IsSuccess.Should().BeTrue();
        user.IsSuspended.Should().BeTrue();
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ReturnsFailure()
    {
        var user = CreateValidUser();
        user.Suspend();

        var result = user.Suspend();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UserAlreadySuspended);
    }

    // ── Delete ────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_RevokesAllActiveTokensAndSetsDeletedAt()
    {
        var user = CreateValidUser();
        user.AddRefreshToken("token-1");
        user.AddRefreshToken("token-2");

        var result = user.Delete();

        result.IsSuccess.Should().BeTrue();
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.RefreshTokens.All(t => !t.IsActive).Should().BeTrue();
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ReturnsFailure()
    {
        var user = CreateValidUser();
        user.Delete();

        var result = user.Delete();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.AccountAlreadyDeleted);
    }

    [Fact]
    public void Block_WhenDeleted_ReturnsFailure()
    {
        var user = CreateValidUser();
        user.Delete();

        var result = user.Block();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.AccountAlreadyDeleted);
    }

    // ── Profile update ────────────────────────────────────────────────────

    [Fact]
    public void UpdateProfile_WithValidData_ReturnsSuccess()
    {
        var user = CreateValidUser();

        var result = user.UpdateProfile("Jane", "Smith", "+1234567890");

        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.UpdatedAt.Should().NotBeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static User CreateValidUser()
    {
        var result = User.Create("test@example.com", "Test", "User", null, UserRole.Client);
        return result.Value;
    }
}
