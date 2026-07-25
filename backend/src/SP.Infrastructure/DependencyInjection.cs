using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SP.Application.Abstractions;
using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Authorization;
using SP.Application.Abstractions.Files;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Repositories;
using SP.Domain.Categories.Repositories;
using SP.Domain.Notifications.Repositories;
using SP.Domain.Portfolio.Repositories;
using SP.Domain.ProviderProfiles.Repositories;
using SP.Domain.Chat.Repositories;
using SP.Domain.Connects.Repositories;
using SP.Domain.Services.Repositories;
using SP.Domain.Payments.Repositories;
using SP.Domain.Shared;
using SP.Domain.Users;
using SP.Application.Abstractions.Payments;
using SP.Infrastructure.Payments.Chargily;
using SP.Infrastructure.Authentication;
using SP.Infrastructure.Authorization;
using SP.Infrastructure.BackgroundJobs;
using SP.Infrastructure.Email;
using SP.Infrastructure.Files;
using SP.Infrastructure.Messaging;
using SP.Infrastructure.Persistence;
using SP.Infrastructure.Persistence.Repositories;
using SP.Infrastructure.Logging;
using SP.Infrastructure.Observability;
using SP.Infrastructure.Services;
using SP.Infrastructure.SignalR;
using SP.Application.Abstractions.Logging;
using SP.Application.Abstractions.Metrics;
using StackExchange.Redis;

namespace SP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── App Settings ──────────────────────────────────────────────────
        services.AddSingleton<IAppSettings, AppSettings>();

        // ── Database ──────────────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(15), errorNumbersToAdd: null);
                sql.CommandTimeout(60);
            }));

        // ── Unit of Work ──────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Repositories ──────────────────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IProviderProfileRepository, ProviderProfileRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IConnectTransactionRepository, ConnectTransactionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IAppConfigurationRepository, AppConfigurationRepository>();
        services.AddScoped<IConnectPackRepository, ConnectPackRepository>();

        // ── Payments ──────────────────────────────────────────────────────
        services.Configure<ChargilyOptions>(configuration.GetSection(ChargilyOptions.SectionName));
        services.AddHttpClient<IPaymentGateway, ChargilyPaymentGateway>();

        // ── Authentication ────────────────────────────────────────────────
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        // ── Authorization ─────────────────────────────────────────────────
        services.AddScoped<IPermissionService, PermissionService>();

        // ── Email ─────────────────────────────────────────────────────────
        services.AddScoped<IEmailService, EmailService>();

        // ── File Storage ──────────────────────────────────────────────────
        services.AddScoped<IFileService, FileService>();

        // ── CQRS Dispatcher ───────────────────────────────────────────────
        services.AddScoped<IDispatcher, Dispatcher>();

        // ── Redis ─────────────────────────────────────────────────────────
        var redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddScoped<IPresenceService, RedisPresenceService>();
        services.AddScoped<IPushNotificationService, StubPushNotificationService>();

        // ── SignalR with Redis backplane ───────────────────────────────────
        services.AddSignalR()
            .AddStackExchangeRedis(redisConnection, options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal("sp-signalr");
            });

        // ── Notification services ─────────────────────────────────────────
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IMessageNotificationService, MessageNotificationService>();

        // ── Observability ─────────────────────────────────────────────────
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddSingleton<IMetricsService, MetricsService>();

        // ── Background Jobs ───────────────────────────────────────────────
        services.AddHostedService<ExpirePendingBookingsJob>();
        services.AddHostedService<CleanupExpiredRefreshTokensJob>();

        return services;
    }
}
