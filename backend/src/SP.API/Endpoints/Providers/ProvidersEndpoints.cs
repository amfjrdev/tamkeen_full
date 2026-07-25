using Microsoft.AspNetCore.Mvc;
using SP.API.Auth;
using SP.API.Extensions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Providers.Queries.GetProviderProfile;
using SP.Application.Providers.Queries.SearchProviders;
using SP.Application.Providers.Queries.GetNearbyProviders;
using SP.Application.Providers.Queries.GetProviderRating;
using SP.Application.Providers.Commands.UpdateAvailability;
using SP.Application.Providers.Commands.UpdateProviderProfile;

namespace SP.API.Endpoints.Providers;

public sealed record UpdateAvailabilityRequest(bool IsAvailable);

public sealed record UpdateProviderProfileRequest(
    string? AboutMe,
    decimal HourlyRate,
    int ResponseTimeMins);

public static class ProvidersEndpoints
{
    public static RouteGroupBuilder MapProvidersEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", SearchProviders).AllowAnonymous();
        group.MapGet("/nearby", GetNearby).AllowAnonymous();
        group.MapGet("/{id:guid}/profile", GetProfile).AllowAnonymous();
        group.MapGet("/{providerId:guid}/rating", GetProviderRating).AllowAnonymous();
        group.MapPut("/availability", UpdateAvailability).RequireAuthorization(AuthorizationPolicies.ProviderOnly);
        group.MapPut("/profile", UpdateProviderProfile).RequireAuthorization(AuthorizationPolicies.ProviderOnly);

        return group;
    }

    private static async Task<IResult> SearchProviders(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] double? minRating = null,
        [FromQuery] bool? availableOnly = null,
        [FromQuery] decimal? priceMin = null,
        [FromQuery] decimal? priceMax = null,
        [FromQuery] double? distanceMaxKm = null,
        [FromQuery] string? lat = null,
        [FromQuery] string? lng = null,
        IDispatcher dispatcher = null!,
        CancellationToken ct = default)
    {
        double? latitude = null;
        if (!string.IsNullOrEmpty(lat))
        {
            if (double.TryParse(lat, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedLat))
                latitude = parsedLat;
            else
                return Results.BadRequest("Invalid lat format. Use dot (.) as decimal separator.");
        }

        double? longitude = null;
        if (!string.IsNullOrEmpty(lng))
        {
            if (double.TryParse(lng, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedLng))
                longitude = parsedLng;
            else
                return Results.BadRequest("Invalid lng format. Use dot (.) as decimal separator.");
        }

        var query = new SearchProvidersQuery(
            categoryId,
            search,
            page,
            pageSize,
            sortBy,
            minRating,
            availableOnly,
            priceMin,
            priceMax,
            distanceMaxKm,
            latitude,
            longitude);

        var result = await dispatcher.QueryAsync(query, ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetNearby(
        [FromQuery] string lat,
        [FromQuery] string lng,
        [FromQuery] double radiusKm = 15.0,
        [FromQuery] string? categoryId = null,
        IDispatcher dispatcher = null!,
        CancellationToken ct = default)
    {
        if (!double.TryParse(lat, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var latitude) ||
            !double.TryParse(lng, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var longitude))
        {
            return Results.BadRequest("Invalid lat or lng format. Use dot (.) as decimal separator.");
        }

        var query = new GetNearbyProvidersQuery(latitude, longitude, radiusKm, categoryId);
        var result = await dispatcher.QueryAsync(query, ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetProfile(
        Guid id,
        string? lat,
        string? lng,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        double? latitude = null;
        if (!string.IsNullOrEmpty(lat))
        {
            if (double.TryParse(lat, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedLat))
                latitude = parsedLat;
            else
                return Results.BadRequest("Invalid lat format. Use dot (.) as decimal separator.");
        }

        double? longitude = null;
        if (!string.IsNullOrEmpty(lng))
        {
            if (double.TryParse(lng, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedLng))
                longitude = parsedLng;
            else
                return Results.BadRequest("Invalid lng format. Use dot (.) as decimal separator.");
        }

        var result = await dispatcher.QueryAsync(
            new GetProviderProfileQuery(id, latitude, longitude), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateAvailability(
        [FromBody] UpdateAvailabilityRequest request,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new UpdateAvailabilityCommand(currentUser.UserId, request.IsAvailable), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> UpdateProviderProfile(
        [FromBody] UpdateProviderProfileRequest request,
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new UpdateProviderProfileCommand(
                currentUser.UserId,
                request.AboutMe,
                request.HourlyRate,
                request.ResponseTimeMins), ct);

        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.NoContent();
    }

    private static async Task<IResult> GetProviderRating(
        Guid providerId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetProviderRatingQuery(providerId), ct);
        return result.IsFailure
            ? result.Error.ToProblem()
            : Results.Ok(result.Value);
    }
}
