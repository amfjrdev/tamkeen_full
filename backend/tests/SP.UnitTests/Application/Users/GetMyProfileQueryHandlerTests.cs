using FluentAssertions;
using NSubstitute;
using SP.Application.Users.Queries.GetMyProfile;
using SP.Domain.Users;
using SP.Domain.Users.Errors;
using Xunit;

namespace SP.UnitTests.Application.Users;

public sealed class GetMyProfileQueryHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();

    [Fact]
    public async Task HandleAsync_WhenUserExists_ReturnsSuccessWithMappedDto()
    {
        // Arrange
        var user = CreateUser();
        _userRepo.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        var handler = new GetMyProfileQueryHandler(_userRepo);
        var query = new GetMyProfileQuery(user.Id);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Email.Should().Be(user.Email);
        result.Value.FirstName.Should().Be(user.FirstName);
        result.Value.LastName.Should().Be(user.LastName);
        result.Value.Role.Should().Be(user.Role.ToString());
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);
        var handler = new GetMyProfileQueryHandler(_userRepo);
        var query = new GetMyProfileQuery(userId);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    private static User CreateUser()
        => User.Create("test@example.com", "Test", "User", null, UserRole.Client).Value;
}
