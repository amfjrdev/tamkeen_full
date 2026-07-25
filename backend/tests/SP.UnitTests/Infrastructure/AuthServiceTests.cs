using FluentAssertions;
using Microsoft.Extensions.Options;
using SP.Domain.Users;
using SP.Infrastructure.Authentication;
using Xunit;

namespace SP.UnitTests.Infrastructure;

public sealed class JwtServiceTests
{
    private readonly JwtService _sut;

    public JwtServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "test-secret-key-minimum-32-characters-long!",
            Issuer = "SP.API",
            Audience = "SP.Client",
            ExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });
        _sut = new JwtService(options);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyToken()
    {
        var user = CreateUser();

        var token = _sut.GenerateAccessToken(user);

        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserClaims()
    {
        var user = CreateUser();

        var token = _sut.GenerateAccessToken(user);

        // Decode payload (base64)
        var parts = token.Split('.');
        var payload = System.Text.Encoding.UTF8.GetString(
            Convert.FromBase64String(PadBase64(parts[1])));

        payload.Should().Contain(user.Id.ToString());
        payload.Should().Contain(user.Email);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        token1.Should().NotBe(token2);
        token1.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_IsBase64Encoded()
    {
        var token = _sut.GenerateRefreshToken();

        var act = () => Convert.FromBase64String(token);
        act.Should().NotThrow();
    }

    private static User CreateUser()
        => User.Create("test@example.com", "Test", "User", null, UserRole.Client).Value;

    private static string PadBase64(string base64)
    {
        var padded = base64.Replace('-', '+').Replace('_', '/');
        return (padded.Length % 4) switch
        {
            2 => padded + "==",
            3 => padded + "=",
            _ => padded
        };
    }
}

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ReturnsDifferentHashForSamePassword()
    {
        var hash1 = _sut.Hash("password123");
        var hash2 = _sut.Hash("password123");

        hash1.Should().NotBe(hash2); // BCrypt uses random salt
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("password123");

        var result = _sut.Verify(hash, "password123");

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("password123");

        var result = _sut.Verify(hash, "wrongpassword");

        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_ProducesBCryptFormat()
    {
        var hash = _sut.Hash("password123");

        hash.Should().StartWith("$2a$"); // BCrypt prefix
    }
}
