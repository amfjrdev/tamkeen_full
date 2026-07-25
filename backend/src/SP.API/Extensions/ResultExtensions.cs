using Microsoft.AspNetCore.Http.HttpResults;
using SP.Domain.Abstractions;

namespace SP.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblem(this Error error)
    {
        var statusCode = ResolveStatusCode(error.Code);

        return TypedResults.Problem(
            title: GetTitle(statusCode),
            detail: error.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }

    private static int ResolveStatusCode(string code) => code switch
    {
        // 404 Not Found
        var c when c.EndsWith(".NotFound", StringComparison.OrdinalIgnoreCase) => 404,

        // 409 Conflict
        var c when c.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("DuplicateName", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadyInUse", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadyVerified", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadyBlocked", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadySuspended", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadyReviewed", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadyReported", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadyCancelled", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadyCompleted", StringComparison.OrdinalIgnoreCase) => 409,
        var c when c.Contains("AlreadyRejected", StringComparison.OrdinalIgnoreCase) => 409,

        // 401 Unauthorized
        var c when c.Contains("InvalidCredentials", StringComparison.OrdinalIgnoreCase) => 401,
        var c when c.Contains("RefreshTokenNotActive", StringComparison.OrdinalIgnoreCase) => 401,
        var c when c.Contains("RefreshTokenNotFound", StringComparison.OrdinalIgnoreCase) => 401,

        // 403 Forbidden
        var c when c.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) => 403,
        var c when c.Contains("IsBlocked", StringComparison.OrdinalIgnoreCase) => 403,
        var c when c.Contains("IsSuspended", StringComparison.OrdinalIgnoreCase) => 403,
        var c when c.Contains("AccountAlreadyDeleted", StringComparison.OrdinalIgnoreCase) => 403,

        // 400 Bad Request (default)
        _ => 400
    };

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        _ => "Request Failed"
    };
}
