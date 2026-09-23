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
using SP.Domain.ServiceRequests.Repositories;
using SP.Domain.Users;

namespace SP.Application.ServiceRequests.Queries.GetProviderApplications;

public sealed record GetProviderApplicationsQuery(
    Guid ProviderId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedList<ServiceRequestSummaryDto>>;

public sealed class GetProviderApplicationsQueryHandler
    : IQueryHandler<GetProviderApplicationsQuery, PagedList<ServiceRequestSummaryDto>>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetProviderApplicationsQueryHandler(
        IServiceRequestRepository requestRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PagedList<ServiceRequestSummaryDto>>> HandleAsync(
        GetProviderApplicationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var (requests, totalCount) = await _requestRepository.GetProviderAppliedRequestsAsync(
            query.ProviderId,
            page,
            pageSize,
            cancellationToken);

        var categories = (await _categoryRepository.GetAllAsync(cancellationToken))
            .ToDictionary(c => c.Id, c => c.Name);

        var clientIds = requests.Select(r => r.ClientId).Distinct().ToList();
        var clients = new Dictionary<Guid, (string Name, string? Avatar)>();
        foreach (var cId in clientIds)
        {
            var u = await _userRepository.GetByIdAsync(cId, cancellationToken);
            if (u != null)
                clients[cId] = ($"{u.FirstName} {u.LastName}".Trim(), u.UserProfilePicture?.Url);
        }

        var items = requests.Select(r =>
        {
            var (cName, cAvatar) = clients.TryGetValue(r.ClientId, out var val) ? val : ("Client", null);
            return new ServiceRequestSummaryDto(
                r.Id,
                r.ClientId,
                cName,
                cAvatar,
                r.CategoryId,
                categories.TryGetValue(r.CategoryId, out var catName) ? catName : "Service",
                r.Title,
                r.Description,
                r.Wilaya,
                r.Budget,
                r.Status.ToString(),
                r.RejectionReason,
                r.SelectedProviderId,
                null,
                r.Applications.Count,
                r.CreatedAt,
                r.ApprovedAt,
                r.CompletedAt);
        }).ToList();

        return Result.Success(new PagedList<ServiceRequestSummaryDto>(items, page, pageSize, totalCount));
    }
}
