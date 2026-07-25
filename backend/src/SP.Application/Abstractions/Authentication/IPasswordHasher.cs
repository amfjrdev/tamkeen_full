using System.Security.Claims;
using SP.Domain.Users;

namespace SP.Application.Abstractions.Authentication;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hashedPassword, string plainPassword);
}
