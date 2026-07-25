// SP.Infrastructure/Authentication/AuthenticationService.cs

using Microsoft.Extensions.Options;
using SP.Application.Abstractions.Authentication;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;
using DomainEmail = SP.Domain.Shared.Email;

namespace SP.Infrastructure.Authentication;

internal sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<AuthenticationResult>> RegisterAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default)
    {
        var hashedPassword = _passwordHasher.Hash(password);

        var credentialResult = user.SetCredential(hashedPassword);
        if (credentialResult.IsFailure)
            return Result.Failure<AuthenticationResult>(credentialResult.Error);

        var authResult = IssueTokens(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(authResult);
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(
        DomainEmail email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email.Value, cancellationToken);
        if (user is null || user.Credential is null)
            return Result.Failure<AuthenticationResult>(UserErrors.InvalidCredentials);

        if (!_passwordHasher.Verify(user.Credential.PasswordHash, password))
            return Result.Failure<AuthenticationResult>(UserErrors.InvalidCredentials);

        if (user.IsBlocked)
            return Result.Failure<AuthenticationResult>(UserErrors.UserIsBlocked);

        if (user.IsSuspended)
            return Result.Failure<AuthenticationResult>(UserErrors.UserIsSuspended);

        if (user.IsDeleted)
            return Result.Failure<AuthenticationResult>(UserErrors.AccountAlreadyDeleted);

        var authResult = IssueTokens(user);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(authResult);
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);
        if (user is null)
            return Result.Failure<AuthenticationResult>(UserErrors.RefreshTokenNotFound);

        var existingToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (existingToken is null || !existingToken.IsActive)
            return Result.Failure<AuthenticationResult>(UserErrors.RefreshTokenNotActive);

        var revokeResult = user.RevokeRefreshToken(refreshToken);
        if (revokeResult.IsFailure)
            return Result.Failure<AuthenticationResult>(revokeResult.Error);

        var authResult = IssueTokens(user);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(authResult);
    }

    public async Task<Result> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.RefreshTokenNotFound);

        var revokeResult = user.RevokeRefreshToken(refreshToken);
        if (revokeResult.IsFailure)
            return Result.Failure(revokeResult.Error);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private AuthenticationResult IssueTokens(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var rawRefreshToken = _jwtService.GenerateRefreshToken();

        var addResult = user.AddRefreshToken(rawRefreshToken, _jwtOptions.RefreshTokenExpirationDays);
        if (addResult.IsFailure)
            throw new InvalidOperationException(addResult.Error.Message);

        // Do NOT call _userRepository.Update() here.
        // For new users the entity is tracked as Added — Update() would flip it to Modified,
        // dropping the INSERT and causing FK violations on UserCredentials/RefreshTokens.
        // For existing users (login/refresh) the entity is already tracked as Modified by EF.

        return new AuthenticationResult(accessToken, rawRefreshToken, user);
    }
}
