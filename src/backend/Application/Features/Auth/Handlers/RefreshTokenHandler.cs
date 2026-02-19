using Application.Common;
using Application.Configuration;
using Application.Features.Auth.Commands;
using Application.Features.Auth.DTOs;
using Application.Interfaces.Auth;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Handlers;

/// <summary>
/// Handler for refreshing authentication tokens.
/// Implements token rotation: the old refresh token is revoked and a new pair is issued.
/// If a revoked token is reused, all tokens for that user are revoked (theft detection).
/// </summary>
public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtConfiguration _jwtConfig;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IOptions<JwtConfiguration> jwtConfig,
        ILogger<RefreshTokenHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _jwtConfig = jwtConfig.Value;
        _logger = logger;
    }

    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            string tokenHash = _jwtTokenService.HashToken(request.RefreshToken);
            RefreshToken? existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (existingToken == null)
            {
                return Result<AuthTokenDto>.Failure("Invalid refresh token.");
            }

            // If the token has been revoked, this could be a token theft attempt
            // Revoke all tokens for this user as a safety measure
            if (existingToken.IsRevoked)
            {
                _logger.LogWarning("Reuse of revoked refresh token detected for user {UserId}. Revoking all tokens.", existingToken.UserId);
                await _refreshTokenRepository.RevokeAllByUserIdAsync(existingToken.UserId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<AuthTokenDto>.Failure("Token has been revoked. Please log in again.");
            }

            if (existingToken.IsExpired)
            {
                return Result<AuthTokenDto>.Failure("Refresh token has expired. Please log in again.");
            }

            // Load the user
            User? user = await _userRepository.GetByIdAsync(existingToken.UserId);
            if (user == null || !user.IsActive)
            {
                return Result<AuthTokenDto>.Failure("User account is not available.");
            }

            // Generate new token pair
            (string accessToken, DateTime expiresAt) = _jwtTokenService.GenerateAccessToken(user);

            string rawNewRefreshToken = _jwtTokenService.GenerateRefreshToken();
            string newRefreshTokenHash = _jwtTokenService.HashToken(rawNewRefreshToken);
            DateTime refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationDays);

            RefreshToken newRefreshToken = new(user.Id, newRefreshTokenHash, refreshTokenExpiresAt);

            // Rotate: revoke old token and link to the new one
            existingToken.Revoke(newRefreshToken.Id);
            await _refreshTokenRepository.UpdateAsync(existingToken);
            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tokens refreshed for user {Email}.", user.Email);

            AuthTokenDto tokenDto = new(accessToken, rawNewRefreshToken, expiresAt);
            return Result<AuthTokenDto>.Success(tokenDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Result<AuthTokenDto>.Failure("An error occurred while refreshing the token.");
        }
    }
}
