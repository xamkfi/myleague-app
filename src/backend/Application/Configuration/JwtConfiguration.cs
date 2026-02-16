namespace Application.Configuration;

/// <summary>
/// Configuration for JWT token generation
/// </summary>
public class JwtConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// The JWT issuer
    /// </summary>
    public string Issuer { get; set; } = "MyLeague";

    /// <summary>
    /// The JWT audience
    /// </summary>
    public string Audience { get; set; } = "MyLeague";

    /// <summary>
    /// The secret key for signing tokens (must be at least 32 characters)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration in minutes
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token expiration in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
