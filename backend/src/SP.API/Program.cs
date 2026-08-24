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
    await ApplyMigrations.ApplyMigrationsAsync(app.Services);

    // ── Middleware pipeline ───────────────────────────────────────────────
    var forwardedOptions = new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    };
    forwardedOptions.KnownNetworks.Clear();
    forwardedOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedOptions);

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
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
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
        c.InjectStylesheet("../swagger-dark.css");
        c.InjectJavascript("../swagger-theme-toggle.js");
        c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
        {
            ["theme"] = "monokai"
        };
    });

        app.MapGet("/reset-password", async (HttpContext context) =>
    {
        var token = context.Request.Query["token"].ToString();
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync($$"""
        <!DOCTYPE html>
        <html lang="ar" dir="rtl">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>إعادة تعيين كلمة المرور - تمكين</title>
            <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700&display=swap" rel="stylesheet">
            <style>
                body {
                    font-family: 'Cairo', sans-serif;
                    background: linear-gradient(135deg, #8A2BE2 0%, #4169E1 100%);
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0;
                    padding: 20px;
                    box-sizing: border-box;
                }
                .card {
                    background: white;
                    padding: 40px;
                    border-radius: 24px;
                    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
                    width: 100%;
                    max-width: 400px;
                    text-align: center;
                }
                h2 {
                    color: #1e1e2f;
                    margin-bottom: 8px;
                    font-weight: 700;
                }
                p {
                    color: #666;
                    font-size: 14px;
                    margin-bottom: 24px;
                }
                .form-group {
                    text-align: right;
                    margin-bottom: 20px;
                }
                label {
                    display: block;
                    margin-bottom: 8px;
                    font-weight: 600;
                    color: #333;
                    font-size: 14px;
                }
                input {
                    width: 100%;
                    padding: 12px 16px;
                    border: 1px solid #ddd;
                    border-radius: 12px;
                    font-size: 16px;
                    box-sizing: border-box;
                    transition: border-color 0.2s;
                }
                input:focus {
                    outline: none;
                    border-color: #8A2BE2;
                }
                button {
                    width: 100%;
                    padding: 14px;
                    border: none;
                    border-radius: 12px;
                    background: linear-gradient(90deg, #8A2BE2 0%, #4169E1 100%);
                    color: white;
                    font-size: 16px;
                    font-weight: 700;
                    cursor: pointer;
                    transition: opacity 0.2s;
                }
                button:hover {
                    opacity: 0.9;
                }
                .error {
                    color: red;
                    font-size: 14px;
                    margin-top: 10px;
                    display: none;
                }
                .success {
                    color: green;
                    font-size: 16px;
                    font-weight: bold;
                    display: none;
                }
            </style>
        </head>
        <body>
            <div class="card" id="formCard">
                <h2>إعادة تعيين كلمة المرور</h2>
                <p>الرجاء إدخال كلمة المرور الجديدة الخاصة بك لحساب تمكين</p>
                <form id="resetForm">
                    <div class="form-group">
                        <label for="password">كلمة المرور الجديدة</label>
                        <input type="password" id="password" required placeholder="كلمة المرور (8 أحرف على الأقل)">
                    </div>
                    <div class="form-group">
                        <label for="confirmPassword">تأكيد كلمة المرور</label>
                        <input type="password" id="confirmPassword" required placeholder="تأكيد كلمة المرور">
                    </div>
                    <button type="submit" id="submitBtn">تحديث كلمة المرور</button>
                    <div class="error" id="errorMsg"></div>
                </form>
            </div>
            <div class="card" id="successCard" style="display:none;">
                <div style="font-size: 48px; margin-bottom: 16px;">✅</div>
                <h2>تم إعادة التعيين بنجاح!</h2>
                <p>لقد تم تحديث كلمة المرور الخاصة بك بنجاح. يمكنك الآن تسجيل الدخول من تطبيق تمكين.</p>
            </div>

            <script>
                const token = "{{token}}";
                const resetForm = document.getElementById('resetForm');
                const formCard = document.getElementById('formCard');
                const successCard = document.getElementById('successCard');
                const errorMsg = document.getElementById('errorMsg');
                const submitBtn = document.getElementById('submitBtn');

                resetForm.addEventListener('submit', async (e) => {
                    e.preventDefault();
                    errorMsg.style.display = 'none';

                    const password = document.getElementById('password').value;
                    const confirmPassword = document.getElementById('confirmPassword').value;

                    if (password.length < 8) {
                        errorMsg.textContent = 'يجب أن تتكون كلمة المرور من 8 أحرف على الأقل';
                        errorMsg.style.display = 'block';
                        return;
                    }

                    if (password !== confirmPassword) {
                        errorMsg.textContent = 'كلمات المرور غير متطابقة';
                        errorMsg.style.display = 'block';
                        return;
                    }

                    submitBtn.disabled = true;
                    submitBtn.textContent = 'جاري التحديث...';

                    try {
                        const response = await fetch('/api/auth/reset-password', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({ token: token, newPassword: password })
                        });

                        if (response.ok) {
                            formCard.style.display = 'none';
                            successCard.style.display = 'block';
                        } else {
                            const errData = await response.json();
                            errorMsg.textContent = errData.detail || 'حدث خطأ أثناء تحديث كلمة المرور. الرجاء المحاولة مرة أخرى.';
                            errorMsg.style.display = 'block';
                            submitBtn.disabled = false;
                            submitBtn.textContent = 'تحديث كلمة المرور';
                        }
                    } catch (err) {
                        errorMsg.textContent = 'خطأ في الاتصال بالخادم. الرجاء المحاولة لاحقاً.';
                        errorMsg.style.display = 'block';
                        submitBtn.disabled = false;
                        submitBtn.textContent = 'تحديث كلمة المرور';
                    }
                });
            </script>
        </body>
        </html>
        """);
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
