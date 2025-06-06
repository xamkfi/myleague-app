using System;
using System.Collections.Generic;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Commands.NewsArticles;

/// <summary>
/// Command for creating a new news article
/// </summary>
public record CreateNewsArticleCommand(
    string Title,
    string ContentHtml,
    string? Summary = null,
    IReadOnlyList<string>? ImageUrls = null,
    string? Author = null,
    string? Category = null,
    string? SportCategory = null,
    IReadOnlyList<string>? Tags = null) : IRequest<Result<NewsArticleDto>>; 