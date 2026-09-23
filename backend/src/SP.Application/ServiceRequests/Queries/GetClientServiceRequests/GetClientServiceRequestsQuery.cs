using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Common;
using SP.Application.ServiceRequests.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Categories.Repositories;
using SP.Domain.ServiceRequests.Enums;
using SP.Domain.ServiceRequests.Repositories;
using SP.Domain.Users;

namespace SP.Application.ServiceRequests.Queries.GetClientServiceRequests;

public sealed record GetClientServiceRequestsQuery(
    Guid ClientId,
    ServiceRequestStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedList<ServiceRequestSummaryDto>>;

public sealed class GetClientServiceRequestsQueryHandler
    : IQueryHandler<GetClientServiceRequestsQuery, PagedList<ServiceRequestSummaryDto>>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetClientServiceRequestsQueryHandler(
        IServiceRequestRepository requestRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PagedList<ServiceRequestSummaryDto>>> HandleAsync(
        GetClientServiceRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var (requests, totalCount) = await _requestRepository.GetClientRequestsAsync(
            query.ClientId,
            query.Status,
            page,
            pageSize,
            cancellationToken);

        var client = await _userRepository.GetByIdAsync(query.ClientId, cancellationToken);
        var clientName = client != null ? $"{client.FirstName} {client.LastName}".Trim() : string.Empty;
        var clientAvatar = client?.UserProfilePicture?.Url;

        var categories = (await _categoryRepository.GetAllAsync(cancellationToken))
            .ToDictionary(c => c.Id, c => c.Name);

        var providerIds = requests
            .Where(r => r.SelectedProviderId.HasValue)
            .Select(r => r.SelectedProviderId!.Value)
            .Distinct()
            .ToList();

        var providers = new Dictionary<Guid, string>();
        foreach (var pId in providerIds)
        {
            var pUser = await _userRepository.GetByIdAsync(pId, cancellationToken);
            if (pUser != null)
                providers[pId] = $"{pUser.FirstName} {pUser.LastName}".Trim();
        }

        var items = requests.Select(r => new ServiceRequestSummaryDto(
            r.Id,
            r.ClientId,
            clientName,
            clientAvatar,
            r.CategoryId,
            categories.TryGetValue(r.CategoryId, out var catName) ? catName : "Service",
            r.Title,
            r.Description,
            r.Wilaya,
            r.Budget,
            r.Status.ToString(),
            r.RejectionReason,
            r.SelectedProviderId,
            r.SelectedProviderId.HasValue && providers.TryGetValue(r.SelectedProviderId.Value, out var pName) ? pName : null,
            r.Applications.Count,
            r.CreatedAt,
            r.ApprovedAt,
            r.CompletedAt
        )).ToList();

        return Result.Success(new PagedList<ServiceRequestSummaryDto>(items, page, pageSize, totalCount));
    }
}
