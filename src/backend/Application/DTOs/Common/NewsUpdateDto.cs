using System;
using System.Collections.Generic;

namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for updating existing news articles
/// </summary>
public record NewsUpdateDto(
    Guid Id,
    string Title,
    string ContentHtml,
    string? Summary = null,
    IReadOnlyList<string>? ImageUrls = null,
    string? Author = null,
    string? Category = null,
    string? SportCategory = null,
    IReadOnlyList<string>? Tags = null); 