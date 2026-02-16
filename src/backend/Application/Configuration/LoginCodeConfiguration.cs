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
}
