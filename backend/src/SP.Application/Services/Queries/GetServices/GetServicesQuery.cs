using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Common;
using SP.Application.Services.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Services.Repositories;

namespace SP.Application.Services.Queries.GetServices;

public sealed record GetServicesQuery(
    Guid? CategoryId = null,
    string? SearchTerm = null,
    Guid? ProviderId = null,
    double? Latitude = null,
    double? Longitude = null,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedList<ServiceSummaryResponse>>;

public sealed class GetServicesQueryHandler : IQueryHandler<GetServicesQuery, PagedList<ServiceSummaryResponse>>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServicesQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<PagedList<ServiceSummaryResponse>>> HandleAsync(
        GetServicesQuery query,
        CancellationToken cancellationToken = default)
    {
        var (details, totalCount) = await _serviceRepository.GetServicesWithDetailsAsync(
            query.CategoryId,
            query.SearchTerm,
            query.ProviderId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = details.Select(x =>
        {
            double? distanceKm = null;
            if (query.Latitude.HasValue && query.Longitude.HasValue && x.Latitude.HasValue && x.Longitude.HasValue)
            {
                distanceKm = CalculateHaversineKm(
                    query.Latitude.Value,
                    query.Longitude.Value,
                    x.Latitude.Value,
                    x.Longitude.Value);
            }

            return new ServiceSummaryResponse(
                x.Service.Id,
                x.Service.ProviderId,
                x.Service.CategoryId,
                x.Service.Name,
                x.Service.Price,
                x.Service.DurationMinutes,
                x.Service.IsActive,
                x.ProviderName,
                x.ProviderEmail,
                x.ProviderRating,
                x.ProviderReviewCount,
                distanceKm);
        }).ToList();

        return Result.Success(new PagedList<ServiceSummaryResponse>(items, query.Page, query.PageSize, totalCount));
    }

    private static double CalculateHaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
