using Application.Common;
using Application.Features.Auth.Commands;
using Application.Interfaces.Auth;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Handlers;

/// <summary>
/// Handler for revoking a refresh token (logout)
/// </summary>
public class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RevokeTokenHandler> _logger;

    public RevokeTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        ILogger<RevokeTokenHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            string tokenHash = _jwtTokenService.HashToken(request.RefreshToken);
            RefreshToken? existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (existingToken == null)
            {
                return Result<bool>.Failure("Invalid refresh token.");
            }

            if (existingToken.IsRevoked)
            {
                // Already revoked, treat as success
                return Result<bool>.Success(true);
            }

            existingToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(existingToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refresh token revoked for user {UserId}.", existingToken.UserId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            return Result<bool>.Failure("An error occurred while revoking the token.");
        }
    }
}
