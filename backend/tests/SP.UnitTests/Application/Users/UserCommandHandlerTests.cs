using FluentAssertions;
using NSubstitute;
using SP.Application.Abstractions.Authentication;
using SP.Application.Users.Commands.BlockUser;
using SP.Application.Users.Commands.DeleteUserAccount;
using SP.Application.Users.Commands.SuspendUser;
using SP.Application.Users.Commands.UnblockUser;
using SP.Application.Users.Commands.UnsuspendUser;
using SP.Application.Users.Commands.UpdateUserPhoneNumber;
using SP.Application.Users.Commands.UpdateUserProfile;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;
using Xunit;

namespace SP.UnitTests.Application.Users;

public sealed class UserCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    // ── UpdateUserProfile ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUserProfile_WhenUserExists_ReturnsSuccess()
    {
        var user = CreateUser();
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var handler = new UpdateUserProfileCommandHandler(_userRepo, _uow);
        var result = await handler.HandleAsync(
            new UpdateUserProfileCommand(user.Id, "Jane", "Smith", null));

        result.IsSuccess.Should().BeTrue();
        _userRepo.Received(1).Update(user);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenUserNotFound_ReturnsFailure()
    {
        _userRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        var handler = new UpdateUserProfileCommandHandler(_userRepo, _uow);
        var result = await handler.HandleAsync(
            new UpdateUserProfileCommand(Guid.NewGuid(), "Jane", "Smith", null));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    // ── UpdateUserPhoneNumber ─────────────────────────────────────────────

    [Fact]
    public async Task UpdatePhoneNumber_WithValidPhone_ReturnsSuccess()
    {
        var user = CreateUser();
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var handler = new UpdateUserPhoneNumberCommandHandler(_userRepo);
        var result = await handler.HandleAsync(
            new UpdateUserPhoneNumberCommand(user.Id, "+1234567890"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePhoneNumber_WithInvalidPhone_ReturnsFailure()
    {
        var user = CreateUser();
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var handler = new UpdateUserPhoneNumberCommandHandler(_userRepo);
        var result = await handler.HandleAsync(
            new UpdateUserPhoneNumberCommand(user.Id, "abc"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidPhoneNumber);
    }

    // ── BlockUser ─────────────────────────────────────────────────────────

    [Fact]
    public async Task BlockUser_WhenUserExists_ReturnsSuccess()
    {
        var user = CreateUser();
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var handler = new BlockUserCommandHandler(_userRepo, _uow);
        var result = await handler.HandleAsync(
            new BlockUserCommand(Guid.NewGuid(), user.Id, "spam"));

        result.IsSuccess.Should().BeTrue();
        user.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public async Task BlockUser_WhenAlreadyBlocked_ReturnsFailure()
    {
        var user = CreateUser();
        user.Block();
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var handler = new BlockUserCommandHandler(_userRepo, _uow);
        var result = await handler.HandleAsync(
            new BlockUserCommand(Guid.NewGuid(), user.Id, null));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UserAlreadyBlocked);
    }

    // ── UnblockUser ───────────────────────────────────────────────────────

    [Fact]
    public async Task UnblockUser_WhenBlocked_ReturnsSuccess()
    {
        var user = CreateUser();
        user.Block();
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var handler = new UnblockUserCommandHandler(_userRepo, _uow);
        var result = await handler.HandleAsync(new UnblockUserCommand(user.Id));

        result.IsSuccess.Should().BeTrue();
        user.IsBlocked.Should().BeFalse();
    }

    // ── SuspendUser ───────────────────────────────────────────────────────

    [Fact]
    public async Task SuspendUser_WhenNotSuspended_ReturnsSuccess()
    {
        var user = CreateUser();
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var handler = new SuspendUserCommandHandler(_userRepo, _uow);
        var result = await handler.HandleAsync(new SuspendUserCommand(user.Id, "violation"));

        result.IsSuccess.Should().BeTrue();
        user.IsSuspended.Should().BeTrue();
    }

    // ── UnsuspendUser ─────────────────────────────────────────────────────

    [Fact]
    public async Task UnsuspendUser_WhenSuspended_ReturnsSuccess()
    {
        var user = CreateUser();
        user.Suspend();
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var handler = new UnsuspendUserCommandHandler(_userRepo, _uow);
        var result = await handler.HandleAsync(new UnsuspendUserCommand(user.Id));

        result.IsSuccess.Should().BeTrue();
        user.IsSuspended.Should().BeFalse();
    }

    // ── DeleteUserAccount ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAccount_WithCorrectConfirmation_ReturnsSuccess()
    {
        var user = CreateUser();
        user.SetCredential("password_hash");
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Verify("password_hash", "password").Returns(true);

        var handler = new DeleteUserAccountCommandHandler(_userRepo, hasher, _uow);
        var result = await handler.HandleAsync(
            new DeleteUserAccountCommand(user.Id, "DELETE", "password"));

        result.IsSuccess.Should().BeTrue();
        await _userRepo.Received(1).HardDeleteAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAccount_WithWrongConfirmation_ReturnsFailure()
    {
        var user = CreateUser();
        user.SetCredential("password_hash");
        _userRepo.GetByIdAsync(user.Id).Returns(user);

        var hasher = Substitute.For<IPasswordHasher>();

        var handler = new DeleteUserAccountCommandHandler(_userRepo, hasher, _uow);
        var result = await handler.HandleAsync(
            new DeleteUserAccountCommand(user.Id, "WRONG", "password"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidConfirmationText);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static User CreateUser()
        => User.Create("test@example.com", "Test", "User", null, UserRole.Client).Value;
}
