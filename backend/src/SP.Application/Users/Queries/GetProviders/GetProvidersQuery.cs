using System.Collections.Generic;
using System.Linq;
using SP.Application.Abstractions.Messaging;
using SP.Application.Users.Dto;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.ProviderProfiles.Repositories;

namespace SP.Application.Users.Queries.GetProviders;

public sealed record GetProvidersQuery : IQuery<IReadOnlyList<UserResponseDto>>;

public sealed class GetProvidersQueryHandler(
    IUserRepository repository,
    IProviderProfileRepository profileRepository)
    : IQueryHandler<GetProvidersQuery, IReadOnlyList<UserResponseDto>>
{
    public async Task<Result<IReadOnlyList<UserResponseDto>>> HandleAsync(
        GetProvidersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await repository.GetByRoleAsync(UserRole.Provider, cancellationToken);
        
        var providerIds = users.Select(u => u.Id).ToList();
        var profiles = await profileRepository.GetByProviderIdsAsync(providerIds, cancellationToken);
        var profileRates = profiles.ToDictionary(p => p.ProviderId, p => p.HourlyRate);

        var providers = users
            .Select(u => {
                var response = u.ToResponse();
                if (profileRates.TryGetValue(u.Id, out var rate))
                {
                    return response with { HourlyRate = rate };
                }
                return response;
            })
            .ToList();

        return Result.Success<IReadOnlyList<UserResponseDto>>(providers);
    }
}