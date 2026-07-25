using Serilog;
using Serilog.Events;
using SP.API.Extensions;
using SP.API.Middleware;
using SP.Infrastructure.SignalR;

// Bootstrap logger for startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SP API");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: ctx.Configuration["Logging:FilePath"] ?? "logs/sp-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddApplicationServices(builder.Configuration);

    var app = builder.Build();

    // ── Migrations ────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        await ApplyMigrations.ApplyMigrationsAsync(app.Services);
    }
    else
    {
        Log.Information("Skipping automatic database migrations in environment: {Environment}", app.Environment.EnvironmentName);
    }

    // ── Middleware pipeline ───────────────────────────────────────────────
    app.UseStaticFiles();
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        // Exclude health check noise from logs
        opts.GetLevel = (ctx, _, _) =>
            ctx.Request.Path.StartsWithSegments("/health")
                ? LogEventLevel.Verbose
                : LogEventLevel.Information;
    });
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    // Only redirect to HTTPS when running outside a container / reverse proxy.
    // In Docker the TLS termination happens at the load-balancer level.
    if (!app.Environment.IsEnvironment("Production") ||
        string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")))
    {
        app.UseHttpsRedirection();
    }
    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Endpoints ─────────────────────────────────────────────────────────
    app.MapEndpoints();
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/detail", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = HealthCheckResponseWriter.WriteDetailedResponse
    });

    // ── Swagger ───────────────────────────────────────────────────────────
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SP API v1");
        c.InjectStylesheet("/swagger-dark.css");
        c.InjectJavascript("/swagger-theme-toggle.js");
        c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
        {
            ["theme"] = "monokai"
        };
    });

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SP API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program accessible to WebApplicationFactory in integration tests
public partial class Program { }
