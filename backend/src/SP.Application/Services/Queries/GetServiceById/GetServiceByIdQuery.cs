// SP.Application/Services/Queries/GetServiceById/GetServiceByIdQuery.cs

using SP.Application.Abstractions.Messaging;
using SP.Application.Services.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Services.Errors;
using SP.Domain.Services.Repositories;

namespace SP.Application.Services.Queries.GetServiceById;

public sealed record GetServiceByIdQuery(Guid ServiceId) : IQuery<ServiceDetailResponse>;

public sealed class GetServiceByIdQueryHandler : IQueryHandler<GetServiceByIdQuery, ServiceDetailResponse>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceByIdQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<ServiceDetailResponse>> HandleAsync(
        GetServiceByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(query.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<ServiceDetailResponse>(ServiceErrors.ServiceNotFound);

        var response = new ServiceDetailResponse(
            service.Id,
            service.ProviderId,
            service.CategoryId,
            service.Name,
            service.Description,
            service.Price,
            service.DurationMinutes,
            service.IsActive,
            service.CreatedAt,
            service.UpdatedAt);

        return Result.Success(response);
    }
}
