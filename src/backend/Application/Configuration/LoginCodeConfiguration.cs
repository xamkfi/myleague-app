namespace Application.Configuration;

/// <summary>
/// Configuration for login code generation and validation
/// </summary>
public class LoginCodeConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings
    /// </summary>
    public const string SectionName = "LoginCode";

    /// <summary>
    /// Login code expiration in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 10;

    /// <summary>
    /// Length of the generated login code
    /// </summary>
    public int CodeLength { get; set; } = 6;

    /// <summary>
    /// Maximum number of failed verification attempts before the code is locked
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// When enabled, the generated login code is returned in the /api/Auth/login response so the
    /// admin login page can auto-fill it. Intended for local development and trusted internal
    /// environments only. SHOULD ALWAYS BE FALSE IN PUBLIC PRODUCTION ENVIRONMENTS, as it makes
    /// the login code observable to anyone who can call the endpoint with a known email address.
    /// Can be overridden via the environment variable LoginCode__AutoFillLoginCode.
    /// </summary>
    public bool AutoFillLoginCode { get; set; }
}
