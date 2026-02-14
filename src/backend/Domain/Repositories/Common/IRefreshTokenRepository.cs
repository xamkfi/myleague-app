using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository for managing refresh tokens
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Gets a refresh token by its hash
    /// </summary>
    /// <param name="tokenHash">The SHA256 hash of the token</param>
    /// <returns>The refresh token if found, null otherwise</returns>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

    /// <summary>
    /// Gets all active (not revoked, not expired) refresh tokens for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Collection of active refresh tokens</returns>
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId);

    /// <summary>
    /// Adds a new refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token to add</param>
    Task AddAsync(RefreshToken refreshToken);

    /// <summary>
    /// Updates an existing refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token to update</param>
    Task UpdateAsync(RefreshToken refreshToken);

    /// <summary>
    /// Revokes all active refresh tokens for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    Task RevokeAllByUserIdAsync(Guid userId);
}
