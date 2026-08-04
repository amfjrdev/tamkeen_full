using SP.API.Contracts.Common;
using System.Net;
using System.Text.Json;

namespace SP.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorResponse) = exception switch
        {
            ArgumentException or ArgumentNullException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("BadRequest", exception.Message)
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("InternalServerError", "Access to the resource or filesystem path is denied.")
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                new ErrorResponse("NotFound", exception.Message)
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("InvalidOperation", exception.Message)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse(
                    "InternalServerError",
                    _environment.IsDevelopment() ? exception.Message : "An error occurred while processing your request")
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
