namespace Application.DTOs.Auth;

/// <summary>
/// DTO containing authentication tokens returned after successful login verification or token refresh
/// </summary>
public record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
