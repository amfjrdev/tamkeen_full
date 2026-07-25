using SP.Domain.Abstractions;

namespace SP.Domain.Users;

public sealed class UserCredential : Entity
{
    private UserCredential() { }

    private UserCredential(Guid id, Guid userId, string passwordHash) : base(id)
    {
        UserId = userId;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    internal static UserCredential Create(Guid userId, string passwordHash)
        => new(Guid.NewGuid(), userId, passwordHash);

    internal void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}