namespace Application.Configuration;


// ----------------    ADJUSTING PAGINATION FOR ENDPOINTS    ----------------
//    
//     You can adjust the setting for each endpoint in the appsettings.json file
//     It is located in src/backend/WebAPI/appsettings.json
//     For example, to adjust the pagination for the GetAllFloorballPlayers endpoint,
//     you can add the following to the appsettings.json file:
//     "Pagination": {
//         "Resources": {
//             "FloorballPlayers": {
//                 "DefaultPageSize": 10,
//                 "MaxPageSize": 100,
//
// ----------------    ADJUSTING PAGINATION FOR ENDPOINTS    ----------------



/// <summary>
/// Configuration options for pagination across the application
/// </summary>

public class PaginationConfiguration
{
    /// <summary>
    /// Configuration section name for binding
    /// </summary>
    public const string SectionName = "Pagination";
    
    /// <summary>
    /// Resource-specific pagination settings
    /// </summary>
    public Dictionary<string, ResourcePaginationSettings> Resources { get; set; } = new();
    
    /// <summary>
    /// Global default pagination settings
    /// </summary>
    public PaginationDefaults Global { get; set; } = new();
}

/// <summary>
/// Pagination settings for a specific resource type
/// </summary>
public class ResourcePaginationSettings
{
    /// <summary>
    /// Default page size for this resource
    /// </summary>
    public int DefaultPageSize { get; set; }
    
    /// <summary>
    /// Maximum allowed page size for this resource
    /// </summary>
    public int MaxPageSize { get; set; }
    
    /// <summary>
    /// Minimum allowed page size for this resource
    /// </summary>
    public int MinPageSize { get; set; } = 1;
}

/// <summary>
/// Global default pagination settings used as fallback
/// </summary>
public class PaginationDefaults
{
    /// <summary>
    /// Default page size when no resource-specific setting exists
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;
    
    /// <summary>
    /// Maximum allowed page size when no resource-specific setting exists
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
    
    /// <summary>
    /// Minimum allowed page size when no resource-specific setting exists
    /// </summary>
    public int MinPageSize { get; set; } = 1;
} 