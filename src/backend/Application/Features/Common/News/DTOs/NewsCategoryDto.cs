namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for News Category enumeration with display information
/// </summary>
public record NewsArticleCategoryDto(
    string Value,
    string DisplayName,
    string Description); 