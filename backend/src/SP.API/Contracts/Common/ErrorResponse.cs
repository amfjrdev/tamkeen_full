namespace SP.API.Contracts.Common;

public sealed record ErrorResponse(
    string Code,
    string Message,
    Dictionary<string, string[]>? Errors = null
);
