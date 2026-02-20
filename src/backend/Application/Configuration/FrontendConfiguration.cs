namespace Application.Configuration;

/// <summary>
/// Configuration for the frontend application URL, used when building links in emails.
/// </summary>
public class FrontendConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings
    /// </summary>
    public const string SectionName = "Frontend";

    /// <summary>
    /// The base URL of the frontend application (e.g. https://myleague.fi or http://localhost:5173)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
}
