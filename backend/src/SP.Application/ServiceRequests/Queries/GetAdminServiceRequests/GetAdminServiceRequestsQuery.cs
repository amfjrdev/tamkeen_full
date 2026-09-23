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

namespace SP.Application.ServiceRequests.Queries.GetAdminServiceRequests;

public sealed record GetAdminServiceRequestsQuery(
    ServiceRequestStatus? Status = null,
    string? Wilaya = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedList<ServiceRequestSummaryDto>>;

public sealed class GetAdminServiceRequestsQueryHandler
    : IQueryHandler<GetAdminServiceRequestsQuery, PagedList<ServiceRequestSummaryDto>>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetAdminServiceRequestsQueryHandler(
        IServiceRequestRepository requestRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PagedList<ServiceRequestSummaryDto>>> HandleAsync(
        GetAdminServiceRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var (requests, totalCount) = await _requestRepository.GetForAdminAsync(
            query.Status,
            query.Wilaya,
            query.Search,
            page,
            pageSize,
            cancellationToken);

        var categories = (await _categoryRepository.GetAllAsync(cancellationToken))
            .ToDictionary(c => c.Id, c => c.Name);

        var userIds = requests.Select(r => r.ClientId)
            .Concat(requests.Where(r => r.SelectedProviderId.HasValue).Select(r => r.SelectedProviderId!.Value))
            .Distinct()
            .ToList();

        var users = new Dictionary<Guid, (string Name, string? Avatar)>();
        foreach (var uId in userIds)
        {
            var u = await _userRepository.GetByIdAsync(uId, cancellationToken);
            if (u != null)
                users[uId] = ($"{u.FirstName} {u.LastName}".Trim(), u.UserProfilePicture?.Url);
        }

        var items = requests.Select(r =>
        {
            var (cName, cAvatar) = users.TryGetValue(r.ClientId, out var clientInfo) ? clientInfo : ("Client", null);
            var pName = r.SelectedProviderId.HasValue && users.TryGetValue(r.SelectedProviderId.Value, out var provInfo) ? provInfo.Name : null;

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
                pName,
                r.Applications.Count,
                r.CreatedAt,
                r.ApprovedAt,
                r.CompletedAt);
        }).ToList();

        return Result.Success(new PagedList<ServiceRequestSummaryDto>(items, page, pageSize, totalCount));
    }
}
