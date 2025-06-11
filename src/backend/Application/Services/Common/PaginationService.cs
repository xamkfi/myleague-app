using Application.Configuration;
using Microsoft.Extensions.Options;

namespace Application.Services.Common;

/// <summary>
/// Implementation of pagination service that provides resource-specific pagination settings
/// </summary>
public class PaginationService : IPaginationService
{
    private readonly PaginationOptions _options;
    private readonly Dictionary<Type, string> _typeToResourceMap;

    /// <summary>
    /// Initializes a new instance of the PaginationService
    /// </summary>
    /// <param name="options">Pagination configuration options</param>
    public PaginationService(IOptions<PaginationOptions> options)
    {
        _options = options.Value;
        _typeToResourceMap = BuildTypeToResourceMap();
    }

    /// <inheritdoc />
    public PaginationSettings GetPaginationSettings(string resourceKey)
    {
        if (_options.Resources.TryGetValue(resourceKey, out ResourcePaginationSettings? resourceSettings))
        {
            return new PaginationSettings(
                resourceSettings.DefaultPageSize,
                resourceSettings.MaxPageSize,
                resourceSettings.MinPageSize);
        }

        // Fallback to global defaults
        return new PaginationSettings(
            _options.Global.DefaultPageSize,
            _options.Global.MaxPageSize,
            _options.Global.MinPageSize);
    }

    /// <inheritdoc />
    public PaginationSettings GetPaginationSettings<T>() where T : class
    {
        string resourceKey = GetResourceKeyFromType<T>();
        return GetPaginationSettings(resourceKey);
    }

    /// <inheritdoc />
    public int GetDefaultPageSize(string resourceKey) => 
        GetPaginationSettings(resourceKey).DefaultPageSize;

    /// <inheritdoc />
    public int GetMaxPageSize(string resourceKey) => 
        GetPaginationSettings(resourceKey).MaxPageSize;

    /// <inheritdoc />
    public int GetMinPageSize(string resourceKey) => 
        GetPaginationSettings(resourceKey).MinPageSize;

    /// <inheritdoc />
    public bool IsValidPageSize(string resourceKey, int pageSize)
    {
        // pageSize=0 means "use default" - always valid
        if (pageSize == 0) return true;

        PaginationSettings settings = GetPaginationSettings(resourceKey);
        return pageSize >= settings.MinPageSize && pageSize <= settings.MaxPageSize;
    }

    /// <inheritdoc />
    public int ResolvePageSize(string resourceKey, int requestedPageSize)
    {
        // If 0 is requested, return the default
        if (requestedPageSize == 0)
        {
            return GetDefaultPageSize(resourceKey);
        }

        return requestedPageSize;
    }

    /// <summary>
    /// Gets the resource key from a type, using type mapping or fallback logic
    /// </summary>
    /// <typeparam name="T">The type to get resource key for</typeparam>
    /// <returns>Resource key for the type</returns>
    private string GetResourceKeyFromType<T>()
    {
        if (_typeToResourceMap.TryGetValue(typeof(T), out string? resourceKey))
            return resourceKey;
        
        // Fallback: derive from type name by removing common suffixes
        string typeName = typeof(T).Name;
        return typeName.Replace("Dto", "")
                      .Replace("Query", "")
                      .Replace("Response", "")
                      .Replace("Request", "");
    }

    /// <summary>
    /// Builds the mapping from types to resource keys for known types
    /// </summary>
    /// <returns>Dictionary mapping types to resource keys</returns>
    private Dictionary<Type, string> BuildTypeToResourceMap()
    {
        // Note: We'll add mappings here as we identify the actual DTO types
        // For now, this provides the infrastructure for type-based resolution
        return new Dictionary<Type, string>
        {
            // Example mappings - these would be updated as actual DTO types are identified
            // { typeof(NewsArticleListDto), "News" },
            // { typeof(FloorballPlayerDto), "FloorballPlayers" },
            // { typeof(FloorballTeamDto), "FloorballTeams" },
            // { typeof(UserDto), "Users" }
        };
    }
} 