using System;
using System.Collections.Generic;

namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for News entity - complete article view
/// </summary>
public record NewsDto(
    Guid Id,
    string Title,
    string ContentHtml,
    string? Summary,
    IReadOnlyList<string> ImageUrls,
    string? Author,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Category,
    string? SportCategory,
    IReadOnlyList<string> Tags,
    bool IsArchived); 