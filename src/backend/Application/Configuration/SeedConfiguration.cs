namespace Application.Configuration;

/// <summary>
/// Configuration for database seeding at application startup
/// </summary>
public class SeedConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings
    /// </summary>
    public const string SectionName = "Seed";

    /// <summary>
    /// The email address for the admin user to create on startup.
    /// If set and no user with this email exists, an admin Person + User pair will be created.
    /// In Azure, set via environment variable: Seed__AdminEmail
    /// </summary>
    public string? AdminEmail { get; set; }
}
