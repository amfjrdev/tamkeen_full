using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Shared;

namespace SP.Application.Configuration.Queries.GetAppConfig;

public sealed class GetAppConfigQueryHandler
    : IQueryHandler<GetAppConfigQuery, AppConfigResponse>
{
    private readonly IAppConfigurationRepository _configRepository;
    private readonly ILogger<GetAppConfigQueryHandler> _logger;

    public GetAppConfigQueryHandler(
        IAppConfigurationRepository configRepository,
        ILogger<GetAppConfigQueryHandler> _logger)
    {
        _configRepository = configRepository;
        this._logger = _logger;
    }

    public async Task<Result<AppConfigResponse>> HandleAsync(
        GetAppConfigQuery query,
        CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.GetPublicConfigurationsAsync(cancellationToken);
        var configDict = configs.ToDictionary(c => c.Key, c => c.Value);

        var emptyStateMessages = new Dictionary<string, string>
        {
            ["no_conversations"] = GetConfigValue(configDict, "empty.no_conversations") ?? "No conversations yet",
            ["no_services"] = GetConfigValue(configDict, "empty.no_services") ?? "No services found",
            ["no_bookings"] = GetConfigValue(configDict, "empty.no_bookings") ?? "No pending requests",
            ["no_categories"] = GetConfigValue(configDict, "empty.no_categories") ?? "No categories available"
        };

        var featureFlags = new Dictionary<string, bool>
        {
            ["chat_enabled"] = GetBoolConfigValue(configDict, "feature.chat_enabled"),
            ["payments_enabled"] = GetBoolConfigValue(configDict, "feature.payments_enabled"),
            ["notifications_enabled"] = GetBoolConfigValue(configDict, "feature.notifications_enabled"),
            ["map_enabled"] = GetBoolConfigValue(configDict, "feature.map_enabled")
        };

        var response = new AppConfigResponse(
            DefaultCity: GetConfigValue(configDict, "location.default_city") ?? "Algiers",
            DefaultLatitude: GetDoubleConfigValue(configDict, "location.default_latitude"),
            DefaultLongitude: GetDoubleConfigValue(configDict, "location.default_longitude"),
            MaxSearchRadius: GetIntConfigValue(configDict, "search.max_radius_km"),
            MaxPriceFilter: GetDoubleConfigValue(configDict, "search.max_price") == 0.0 ? 10000.0 : GetDoubleConfigValue(configDict, "search.max_price"),
            MaxDistanceFilter: GetDoubleConfigValue(configDict, "search.max_distance") == 0.0 ? 50.0 : GetDoubleConfigValue(configDict, "search.max_distance"),
            RatingFilterOptions: GetDoubleListConfigValue(configDict, "search.rating_options") ?? new List<double> { 0.0, 3.0, 3.5, 4.0, 4.5 },
            CurrencyCode: GetConfigValue(configDict, "currency.code") ?? "DZD",
            CurrencySymbol: GetConfigValue(configDict, "currency.symbol") ?? "DA",
            EmptyStateMessages: emptyStateMessages,
            FeatureFlags: featureFlags,
            MinAppVersion: GetConfigValue(configDict, "app.min_version") ?? "1.0.0",
            MaintenanceMode: GetBoolConfigValue(configDict, "app.maintenance_mode"),
            MaintenanceMessage: GetConfigValue(configDict, "app.maintenance_message")
        );

        return Result.Success(response);
    }

    private static string? GetConfigValue(Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out var value) ? value : null;
    }

    private static bool GetBoolConfigValue(Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out var value) && bool.TryParse(value, out var result) && result;
    }

    private static int GetIntConfigValue(Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : 0;
    }

    private static double GetDoubleConfigValue(Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out var value) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0.0;
    }

    private List<double>? GetDoubleListConfigValue(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            try
            {
                return value.Split(',')
                    .Select(s => double.Parse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse configuration key '{Key}' with value '{Value}'. Falling back to default list.", key, value);
            }
        }
        return null;
    }
}