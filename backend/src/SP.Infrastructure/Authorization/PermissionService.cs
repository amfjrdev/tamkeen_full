// SP.Infrastructure/Authorization/PermissionService.cs

using Microsoft.EntityFrameworkCore;
using SP.Application.Abstractions.Authorization;
using SP.Domain.Users;
using SP.Infrastructure.Persistence;

namespace SP.Infrastructure.Authorization;

internal sealed class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var role = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id == userId)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null) return false;

        return permission switch
        {
            "Admin" => role == UserRole.Admin,
            "Provider" => role == UserRole.Provider || role == UserRole.Admin,
            "Client" => role == UserRole.Client || role == UserRole.Admin,
            _ => false
        };
    }
}
