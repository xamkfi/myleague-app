using System;
using System.Collections.Generic;

namespace Application.Features.Common.News.DTOs;

/// <summary>
/// Data Transfer Object for creating new news articles
/// </summary>
public record NewsArticleCreateDto(
    string Title,
    string ContentHtml,
    string? Summary = null,
    IReadOnlyList<string>? ImageUrls = null,
    string? Author = null,
    string? Category = null,
    string? SportCategory = null,
    IReadOnlyList<string>? Tags = null); 
