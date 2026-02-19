using System;
using System.Collections.Generic;

namespace Application.Features.Common.News.DTOs;

/// <summary>
/// Data Transfer Object for updating existing news articles
/// </summary>
public record NewsArticleUpdateDto(
    Guid Id,
    string Title,
    string ContentHtml,
    string? Summary = null,
    IReadOnlyList<string>? ImageUrls = null,
    string? Author = null,
    string? Category = null,
    string? SportCategory = null,
    IReadOnlyList<string>? Tags = null); 
