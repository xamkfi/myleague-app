using System;
using System.Collections.Generic;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Commands.News;

/// <summary>
/// Command for creating a new news article
/// </summary>
public record CreateNewsCommand(
    string Title,
    string ContentHtml,
    string? Summary = null,
    IReadOnlyList<string>? ImageUrls = null,
    string? Author = null,
    string? Category = null,
    string? SportCategory = null,
    IReadOnlyList<string>? Tags = null) : IRequest<Result<NewsDto>>; 