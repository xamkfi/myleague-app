namespace Application.Configuration;

/// <summary>
/// Configuration for Azure Communication Services Email
/// </summary>
public class AzureCommunicationServicesConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings
    /// </summary>
    public const string SectionName = "AzureCommunicationServices";

    /// <summary>
    /// The Azure Communication Services connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The sender email address (must be verified in Azure)
    /// </summary>
    public string SenderAddress { get; set; } = string.Empty;
}
