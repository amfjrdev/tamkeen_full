using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;

namespace SP.Application.Users.Commands.Register;

public record RegisterCommand(
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Password,
    string Role) : ICommand<AuthenticationResult>;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IAuthenticationService authenticationService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _authenticationService = authenticationService;
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

        return Result.Success(authResult.Value);
    }
}