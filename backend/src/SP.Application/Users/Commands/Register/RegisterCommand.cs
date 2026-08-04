using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;
using SP.Domain.Categories.Repositories;
using SP.Domain.Services;
using SP.Domain.Services.Repositories;
using SP.Domain.ProviderProfiles;
using SP.Domain.ProviderProfiles.Repositories;

namespace SP.Application.Users.Commands.Register;

public record RegisterCommand(
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Password,
    string Role,
    Guid? CategoryId) : ICommand<AuthenticationResult>;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IProviderProfileRepository _providerProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IAuthenticationService authenticationService,
        ICategoryRepository categoryRepository,
        IServiceRepository serviceRepository,
        IProviderProfileRepository providerProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _authenticationService = authenticationService;
        _categoryRepository = categoryRepository;
        _serviceRepository = serviceRepository;
        _providerProfileRepository = providerProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResult>> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Check email uniqueness
        var emailExists = await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken);
        if (emailExists)
            return Result.Failure<AuthenticationResult>(UserErrors.EmailAlreadyInUse);

        // 2. Parse role
        if (!Enum.TryParse<UserRole>(command.Role, ignoreCase: true, out var role))
            return Result.Failure<AuthenticationResult>(UserErrors.InvalidRole);

        // 3. Create user domain object
        var userResult = User.Create(
            command.Email,
            command.FirstName,
            command.LastName,
            command.PhoneNumber,
            role);

        if (userResult.IsFailure)
            return Result.Failure<AuthenticationResult>(userResult.Error);

        var user = userResult.Value;

        // 4. Persist user first so FK exists for credential
        await _userRepository.AddAsync(user, cancellationToken);

        // 5. Register via auth service (hashes password + issues tokens)
        // NOTE: RegisterAsync internally calls SaveChangesAsync — do NOT call it again here
        var authResult = await _authenticationService.RegisterAsync(user, command.Password, cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure<AuthenticationResult>(authResult.Error);

        // 6. If provider and CategoryId is selected, create profile and default service
        if (role == UserRole.Provider && command.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId.Value, cancellationToken);
            if (category is not null)
            {
                // Create Provider Profile
                var profile = ProviderProfile.Create(user.Id);
                profile.SetAvailability(true);
                await _providerProfileRepository.AddAsync(profile, cancellationToken);

                // Create default service named after the category
                var serviceResult = Service.Create(
                    user.Id,
                    category.Id,
                    category.Name,
                    $"General {category.Name} Services",
                    1000m, // default price
                    60 // default duration in minutes
                );

                if (serviceResult.IsSuccess)
                {
                    await _serviceRepository.AddAsync(serviceResult.Value, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return Result.Success(authResult.Value);
    }
}