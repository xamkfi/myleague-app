using System;
using System.Collections.Generic;

namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for News entity - simplified list view without content
/// </summary>
public record NewsListDto(
    Guid Id,
    string Title,
    string? Summary,
    string? Author,
    DateTime CreatedAt,
    string? Category,
    string? SportCategory,
    IReadOnlyList<string> Tags,
    bool IsArchived); 