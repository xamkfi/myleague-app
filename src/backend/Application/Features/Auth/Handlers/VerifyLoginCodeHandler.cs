using Application.Common;
using Application.Configuration;
using Application.Features.Auth.Commands;
using Application.Features.Auth.DTOs;
using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Handlers;

/// <summary>
/// Handler for verifying a login code and issuing authentication tokens.
/// Includes brute-force protection via attempt counting.
/// </summary>
public class VerifyLoginCodeHandler : IRequestHandler<VerifyLoginCodeCommand, Result<AuthTokenDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISiteSettingsProvider _siteSettingsProvider;
    private readonly LoginCodeConfiguration _loginCodeConfig;
    private readonly ILogger<VerifyLoginCodeHandler> _logger;

    public VerifyLoginCodeHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        ISiteSettingsProvider siteSettingsProvider,
        IOptions<LoginCodeConfiguration> loginCodeConfig,
        ILogger<VerifyLoginCodeHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _siteSettingsProvider = siteSettingsProvider;
        _loginCodeConfig = loginCodeConfig.Value;
        _logger = logger;
    }

    public async Task<Result<AuthTokenDto>> Handle(VerifyLoginCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<AuthTokenDto>.Failure("Invalid email or login code.");
            }

            if (!user.IsActive && !_loginCodeConfig.AutoFillLoginCode)
            {
                return Result<AuthTokenDto>.Failure("This account has been deactivated.");
            }

            // Check if there is an active login code
            if (string.IsNullOrEmpty(user.LoginCode) || !user.LoginCodeExpiresAt.HasValue)
            {
                return Result<AuthTokenDto>.Failure("No login code has been requested. Please request a new code.");
            }

            // Check if the code has expired
            if (DateTime.UtcNow >= user.LoginCodeExpiresAt.Value)
            {
                user.ClearLoginCode();
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<AuthTokenDto>.Failure("The login code has expired. Please request a new code.");
            }

            EffectiveAuthSettings authSettings = await _siteSettingsProvider.GetEffectiveAsync(cancellationToken);

            // Check brute-force attempts
            if (user.LoginCodeAttempts >= authSettings.LoginCodeMaxAttempts)
            {
                user.ClearLoginCode();
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<AuthTokenDto>.Failure("Too many failed attempts. Please request a new login code.");
            }

            // Validate the code
            if (!string.Equals(user.LoginCode, request.Code, StringComparison.Ordinal))
            {
                user.IncrementLoginCodeAttempts();
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                int remainingAttempts = authSettings.LoginCodeMaxAttempts - user.LoginCodeAttempts;
                _logger.LogInformation("Failed login code attempt for {Email}. {Remaining} attempts remaining.", request.Email, remainingAttempts);
                return Result<AuthTokenDto>.Failure("Invalid login code.");
            }

            // Code is valid -- generate tokens
            (string accessToken, DateTime expiresAt) = _jwtTokenService.GenerateAccessToken(
                user,
                authSettings.AccessTokenExpirationMinutes);

            string rawRefreshToken = _jwtTokenService.GenerateRefreshToken();
            string refreshTokenHash = _jwtTokenService.HashToken(rawRefreshToken);

            DateTime refreshTokenExpiresAt = DateTime.UtcNow.AddDays(authSettings.RefreshTokenExpirationDays);
            RefreshToken refreshToken = new(user.Id, refreshTokenHash, refreshTokenExpiresAt);

            await _refreshTokenRepository.AddAsync(refreshToken);

            // Record successful login
            user.RecordLogin();
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {Email} logged in successfully.", request.Email);

            AuthTokenDto tokenDto = new(
                accessToken,
                rawRefreshToken,
                expiresAt,
                authSettings.SessionExpiryWarningMinutes);
            return Result<AuthTokenDto>.Success(tokenDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying login code for {Email}", request.Email);
            return Result<AuthTokenDto>.Failure("An error occurred while verifying the login code.");
        }
    }
}
