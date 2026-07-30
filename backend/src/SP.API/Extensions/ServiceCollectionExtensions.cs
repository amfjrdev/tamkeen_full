using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SP.API.Auth;
using SP.Application.Abstractions.Messaging;
using SP.Application.Categories.Commands.CreateCategory;
using SP.Application.Categories.Commands.DeleteCategory;
using SP.Application.Categories.Commands.UpdateCategory;
using SP.Application.Categories.Dtos;
using SP.Application.Categories.Queries.GetAllCategories;
using SP.Application.Categories.Queries.GetCategoryById;
using SP.Application.Users.Commands.Login;
using SP.Application.Users.Dto;
using SP.Application.Users.Queries.GetAllUsers;
using SP.Application.Users.Queries.GetUserById;
using SP.Application.Users.Queries.GetMyProfile;
using SP.Application.Providers.Dtos;
using SP.Application.Providers.Queries.GetProviderProfile;
using SP.Application.Providers.Queries.SearchProviders;
using SP.Application.Providers.Queries.GetNearbyProviders;
using SP.Application.Providers.Queries.GetProviderRating;
using SP.Application.Providers.Commands.UpdateAvailability;
using SP.Application.Chat.Queries.GetConversations;
using SP.Application.Chat.Queries.GetMessages;
using SP.Application.Chat.Queries.GetConversationDetails;
using SP.Application.Chat.Commands.SendMessage;
using SP.Application.Chat.Commands.MarkMessageAsRead;
using SP.Application.Chat.Commands.UnlockConversation;
using SP.Application.Chat.Dtos;
using SP.Application.Connects.Queries.GetConnectBalance;
using SP.Application.Connects.Queries.GetConnectPacks;
using SP.Application.Connects.Commands.PurchaseConnects;
using SP.Application.Connects.Dtos;
using SP.Application.Portfolio.Dtos;
using SP.Application.Portfolio.Queries.GetPortfolioItems;
using SP.Application.Payments.Commands.CreateCheckoutSession;
using SP.Application.Payments.Commands.CompletePayment;
using SP.Application.Payments.Queries.GetPaymentHistory;
using SP.Application.Configuration.Queries.GetAppConfig;
using SP.Application.Configuration.Commands.UpdateConfiguration;
using SP.Infrastructure;
using System.Text;

namespace SP.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddApplicationHandlers();
        services.AddJwtAuthentication(configuration);
        services.AddCurrentUser();
        services.AddValidators();
        services.AddSwaggerWithJwt();
        services.AddCorsPolicy(configuration);
        services.AddRateLimiting(configuration);
        services.AddHealthChecks(configuration);
        services.AddMemoryCache();

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.AdminOnly, p => p.RequireRole(AuthorizationPolicies.AdminRole))
            .AddPolicy(AuthorizationPolicies.ProviderOnly, p => p.RequireRole(AuthorizationPolicies.ProviderRole))
            .AddPolicy(AuthorizationPolicies.ClientOnly, p => p.RequireRole(AuthorizationPolicies.ClientRole));

        return services;
    }

    // ── Current user ──────────────────────────────────────────────────────
    private static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
        services.AddScoped<SP.Application.Abstractions.Authentication.IUserContext>(
            sp => sp.GetRequiredService<CurrentUser>());
        return services;
    }

    // ── FluentValidation ──────────────────────────────────────────────────
    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        return services;
    }

    // ── JWT Authentication ────────────────────────────────────────────────
    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };

                // Allow JWT via query string for SignalR WebSocket connections
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    // ── Swagger ───────────────────────────────────────────────────────────
    private static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Service Provider API",
                Version = "v1",
                Description = "Clean Architecture API — CQRS + Result pattern"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT access token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.TagActionsBy(api => new[] { api.GroupName ?? "Default" });
            options.DocInclusionPredicate((_, _) => true);
        });

        return services;
    }

    // ── CORS ──────────────────────────────────────────────────────────────
    private static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];

                if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                else
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
            });
        });

        return services;
    }

    // ── Rate Limiting ─────────────────────────────────────────────────────
    private static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var globalPermitLimit = configuration.GetValue<int>("RateLimiting:GlobalPermitLimit", 100);
        var globalWindowSeconds = configuration.GetValue<int>("RateLimiting:GlobalWindowSeconds", 10);
        var authPermitLimit = configuration.GetValue<int>("RateLimiting:AuthPermitLimit", 10);
        var authWindowSeconds = configuration.GetValue<int>("RateLimiting:AuthWindowSeconds", 60);

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = globalPermitLimit,
                        Window = TimeSpan.FromSeconds(globalWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddFixedWindowLimiter("auth", o =>
            {
                o.PermitLimit = authPermitLimit;
                o.Window = TimeSpan.FromSeconds(authWindowSeconds);
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit = 0;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    // ── Health Checks ─────────────────────────────────────────────────────
    private static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sqlConn = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        var redisConn = configuration.GetConnectionString("Redis") ?? string.Empty;

        services.AddHealthChecks()
            .AddSqlServer(sqlConn, name: "sqlserver", tags: ["db", "sql"])
            .AddRedis(redisConn, name: "redis", tags: ["cache", "redis"]);

        return services;
    }

    // ── CQRS handler scanning ─────────────────────────────────────────────
    private static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ICommand).Assembly;

        services.Scan(scan => scan
            .FromAssemblies(applicationAssembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblies(applicationAssembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblies(applicationAssembly)
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // ── Explicit registrations for handlers that inherit generic base classes ──
        // The Scrutor scan above registers them under the BASE interface type
        // (e.g. ICommandHandler<CreateCommand<T>, Guid>), but the Dispatcher resolves
        // by the CONCRETE type (e.g. ICommandHandler<CreateCategoryCommand, Guid>).
        // These explicit registrations bridge that gap.

        // Categories — commands
        services.AddScoped<
            ICommandHandler<CreateCategoryCommand, Guid>,
            CreateCategoryCommandHandler>();
        services.AddScoped<
            ICommandHandler<UpdateCategoryCommand>,
            UpdateCategoryCommandHandler>();
        services.AddScoped<
            ICommandHandler<DeleteCategoryCommand>,
            DeleteCategoryCommandHandler>();

        // Categories — queries
        services.AddScoped<
            IQueryHandler<GetAllCategoriesQuery, IReadOnlyList<CategoryResponseDto>>,
            GetAllCategoriesQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetCategoryByIdQuery, CategoryResponseDto?>,
            GetCategoryByIdQueryHandler>();

        // Users — queries
        services.AddScoped<
            IQueryHandler<GetAllUsersQuery, IReadOnlyList<UserResponseDto>>,
            GetAllUsersQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetUserByIdQuery, UserResponseDto?>,
            GetUserByIdQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetMyProfileQuery, UserResponseDto>,
            GetMyProfileQueryHandler>();

        // Providers — queries
        services.AddScoped<
            IQueryHandler<GetProviderProfileQuery, ProviderProfileResponse>,
            GetProviderProfileQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetProviderRatingQuery, ProviderRatingResponse>,
            GetProviderRatingQueryHandler>();
        services.AddScoped<
            IQueryHandler<SearchProvidersQuery, ProviderSearchResponse>,
            SearchProvidersQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetNearbyProvidersQuery, IReadOnlyList<NearbyProviderDto>>,
            GetNearbyProvidersQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetConversationsQuery, IReadOnlyList<ConversationResponse>>,
            GetConversationsQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetMessagesQuery, GetMessagesResponse>,
            GetMessagesQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetConversationDetailsQuery, ConversationResponse>,
            GetConversationDetailsQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetConnectBalanceQuery, ConnectBalanceResponse>,
            GetConnectBalanceQueryHandler>();
        services.AddScoped<
            ICommandHandler<SendMessageCommand, MessageResponse>,
            SendMessageCommandHandler>();
        services.AddScoped<
            ICommandHandler<MarkMessageAsReadCommand>,
            MarkMessageAsReadCommandHandler>();
        services.AddScoped<
            ICommandHandler<UnlockConversationCommand>,
            UnlockConversationCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetConnectPacksQuery, IReadOnlyList<ConnectPackDto>>,
            GetConnectPacksQueryHandler>();
        services.AddScoped<
            ICommandHandler<PurchaseConnectsCommand, ConnectBalanceResponse>,
            PurchaseConnectsCommandHandler>();
        services.AddScoped<
            ICommandHandler<UpdateAvailabilityCommand>,
            UpdateAvailabilityCommandHandler>();

        // Payments
        services.AddScoped<
            ICommandHandler<CreateCheckoutSessionCommand, CheckoutSessionResponse>,
            CreateCheckoutSessionCommandHandler>();
        services.AddScoped<
            ICommandHandler<CompletePaymentCommand>,
            CompletePaymentCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetPaymentHistoryQuery, IReadOnlyList<PaymentHistoryDto>>,
            GetPaymentHistoryQueryHandler>();

        // Portfolio — queries
        services.AddScoped<
            IQueryHandler<GetPortfolioItemsQuery, IReadOnlyList<PortfolioItemResponse>>,
            GetPortfolioItemsQueryHandler>();

        // Configuration — queries
        services.AddScoped<
            IQueryHandler<GetAppConfigQuery, AppConfigResponse>,
            GetAppConfigQueryHandler>();

        // Configuration — commands
        services.AddScoped<
            ICommandHandler<UpdateConfigurationCommand>,
            UpdateConfigurationCommandHandler>();

        return services;
    }
}
