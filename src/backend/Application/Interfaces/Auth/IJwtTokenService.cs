using Domain.Entities.Common;

namespace Application.Interfaces.Auth;

/// <summary>
/// Abstraction for JWT access token and refresh token operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a short-lived JWT access token for the given user
    /// </summary>
    /// <param name="user">The user to generate the token for</param>
    /// <param name="accessTokenExpirationMinutes">Lifetime of the access token in minutes</param>
    /// <returns>A tuple of (token string, expiration UTC time)</returns>
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, int accessTokenExpirationMinutes);

    /// <summary>
    /// Generates a cryptographically random refresh token string
    /// </summary>
    /// <returns>The raw refresh token string (caller is responsible for hashing before storage)</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Computes the SHA256 hash of a token for secure database storage
    /// </summary>
    /// <param name="token">The raw token string</param>
    /// <returns>The SHA256 hash as a base64 string</returns>
    string HashToken(string token);
}
