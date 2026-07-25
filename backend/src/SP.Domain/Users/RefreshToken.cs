using SP.Domain.Abstractions;

namespace SP.Domain.Users;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }

    private RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt) : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    internal static RefreshToken Create(Guid userId, string token, int expirationDays = 7)
        => new(Guid.NewGuid(), userId, token, DateTime.UtcNow.AddDays(expirationDays));

    internal void Revoke()
    {
        if (!IsRevoked)
            RevokedAt = DateTime.UtcNow;
    }
}