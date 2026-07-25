using SP.Domain.Shared;

namespace SP.Infrastructure.Persistence;

public static class ConfigurationSeeder
{
    public static async Task SeedConfigurationsAsync(ApplicationDbContext context)
    {
        if (!context.AppConfigurations.Any())
        {
            var configurations = new List<AppConfiguration>
            {
                // Location settings
                AppConfiguration.Create("location.default_city", "Algiers", "location", true),
                AppConfiguration.Create("location.default_latitude", "36.7372", "location", true),
                AppConfiguration.Create("location.default_longitude", "3.0865", "location", true),
                
                // Search settings
                AppConfiguration.Create("search.max_radius_km", "50", "search", true),
                
                // Empty state messages
                AppConfiguration.Create("empty.no_conversations", "No conversations yet", "empty_states", true),
                AppConfiguration.Create("empty.no_services", "No services found", "empty_states", true),
                AppConfiguration.Create("empty.no_bookings", "No pending requests", "empty_states", true),
                AppConfiguration.Create("empty.no_categories", "No categories available", "empty_states", true),
                
                // Feature flags
                AppConfiguration.Create("feature.chat_enabled", "true", "features", true),
                AppConfiguration.Create("feature.payments_enabled", "true", "features", true),
                AppConfiguration.Create("feature.notifications_enabled", "true", "features", true),
                AppConfiguration.Create("feature.map_enabled", "true", "features", true),
                
                // App settings
                AppConfiguration.Create("app.min_version", "1.0.0", "app", true),
                AppConfiguration.Create("app.maintenance_mode", "false", "app", true),
                AppConfiguration.Create("app.maintenance_message", "", "app", true),

                // Search filters and currency settings
                AppConfiguration.Create("search.max_price", "10000.0", "search", true),
                AppConfiguration.Create("search.max_distance", "50.0", "search", true),
                AppConfiguration.Create("search.rating_options", "0.0,3.0,3.5,4.0,4.5", "search", true),
                AppConfiguration.Create("currency.code", "DZD", "currency", true),
                AppConfiguration.Create("currency.symbol", "DA", "currency", true)
            };

            context.AppConfigurations.AddRange(configurations);
            await context.SaveChangesAsync();
        }
    }
}