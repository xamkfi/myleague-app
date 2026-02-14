using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Auth;

/// <summary>
/// Request model for requesting a login code
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// The email address to send the login code to
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "A valid email address is required")]
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Request model for verifying a login code
/// </summary>
public record VerifyCodeRequest
{
    /// <summary>
    /// The email address the code was sent to
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "A valid email address is required")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The 6-digit login code
    /// </summary>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must contain only digits")]
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Request model for refreshing tokens
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// The refresh token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// Request model for revoking a refresh token (logout)
/// </summary>
public record LogoutRequest
{
    /// <summary>
    /// The refresh token to revoke
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; init; } = string.Empty;
}
