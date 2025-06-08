using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.NewsArticles;

/// <summary>
/// Query for retrieving all news articles with pagination and filtering
/// </summary>
public record GetAllNewsArticlesQuery(
    int Page = 1,
    int PageSize = 3,
    string? Category = null,
    string? SportCategory = null,
    string? Author = null,
    bool IncludeArchived = false) : IRequest<Result<PagedResult<NewsArticleListDto>>>; 