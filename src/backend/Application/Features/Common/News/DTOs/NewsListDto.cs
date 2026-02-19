using System;
using System.Collections.Generic;

namespace Application.Features.Common.News.DTOs;

/// <summary>
/// Data Transfer Object for News entity - simplified list view without content
/// </summary>
public record NewsArticleListDto(
    Guid Id,
    string Title,
    Uri? MainImage,
    string? Summary,
    string? Author,
    DateTime CreatedAt,
    string? Category,
    string? SportCategory,
    IReadOnlyList<string> Tags,
    bool IsArchived); 
