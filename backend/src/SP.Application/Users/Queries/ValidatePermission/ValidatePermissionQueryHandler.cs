using SP.Application.Abstractions.Authorization;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;

namespace SP.Application.Users.Queries.ValidatePermission;

public sealed class ValidatePermissionQueryHandler : IQueryHandler<ValidatePermissionQuery, bool>
{
    private readonly IPermissionService _permissionService;

    public ValidatePermissionQueryHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<Result<bool>> HandleAsync(ValidatePermissionQuery query, CancellationToken cancellationToken = default)
    {
        var hasPermission = await _permissionService.HasPermissionAsync(query.UserId, query.Permission, cancellationToken);
        return Result.Success(hasPermission);
    }
}