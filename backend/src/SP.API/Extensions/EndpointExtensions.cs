using SP.API.Endpoints.Authentication;
using SP.API.Endpoints.Bookings;
using SP.API.Endpoints.Categories;
using SP.API.Endpoints.Chat;
using SP.API.Endpoints.Configuration;
using SP.API.Endpoints.Notifications;
using SP.API.Endpoints.Portfolio;
using SP.API.Endpoints.Providers;
using SP.API.Endpoints.Reports;
using SP.API.Endpoints.Reviews;
using SP.API.Endpoints.Services;
using SP.API.Endpoints.ServiceRequests;
using SP.API.Endpoints.Users;
using SP.API.Endpoints.Connects;
using SP.API.Endpoints.Payments;
using SP.API.Endpoints.AdminDashboard;

namespace SP.API.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api").WithOpenApi();

        api.MapAdminDashboardEndpoints().WithTags("Admin Dashboard");
        api.MapGroup("/auth").MapAuthEndpoints().WithTags("Auth");
        api.MapGroup("/users").MapUsersEndpoints().WithTags("Users");
        api.MapGroup("/services").MapServicesEndpoints().WithTags("Services");
        api.MapGroup("/service-requests").MapServiceRequestsEndpoints().WithTags("Service Requests");
        api.MapGroup("/bookings").MapBookingsEndpoints().WithTags("Bookings");
        api.MapGroup("/categories").MapCategoriesEndpoints().WithTags("Categories");
        api.MapGroup("/portfolio").MapPortfolioEndpoints().WithTags("Portfolio");
        api.MapGroup("/reports").MapReportsEndpoints().WithTags("Reports");
        api.MapGroup("/notifications").MapNotificationsEndpoints().WithTags("Notifications");
        api.MapGroup("/providers").MapProvidersEndpoints().WithTags("Providers");
        api.MapGroup("/reviews").MapReviewsEndpoints().WithTags("Reviews");
        api.MapGroup("/chat").MapChatEndpoints().WithTags("Chat");
        api.MapGroup("/connects").MapConnectsEndpoints().WithTags("Connects");
        api.MapGroup("/payments").MapPaymentsEndpoints().WithTags("Payments");
        api.MapGroup("/config").MapConfigurationEndpoints().WithTags("Configuration");
        api.MapGroup("/admin/config").MapAdminConfigurationEndpoints().WithTags("Admin Configuration");

        return app;
    }
}
