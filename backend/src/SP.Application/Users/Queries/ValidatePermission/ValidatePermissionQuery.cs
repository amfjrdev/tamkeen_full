using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Queries.ValidatePermission;

public sealed record ValidatePermissionQuery(Guid UserId, string Permission) : IQuery<bool>;