using System.Collections.Generic;

namespace SP.Application.Configuration.Queries.GetAppConfig;

public sealed record AppConfigResponse(
    string DefaultCity,
    double DefaultLatitude,
    double DefaultLongitude,
    int MaxSearchRadius,
    double MaxPriceFilter,
    double MaxDistanceFilter,
    List<double> RatingFilterOptions,
    string CurrencyCode,
    string CurrencySymbol,
    Dictionary<string, string> EmptyStateMessages,
    Dictionary<string, bool> FeatureFlags,
    string MinAppVersion,
    bool MaintenanceMode,
    string? MaintenanceMessage
);
