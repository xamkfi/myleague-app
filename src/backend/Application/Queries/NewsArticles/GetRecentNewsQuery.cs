using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.NewsArticles;

/// <summary>
/// Query for retrieving recent news articles
/// </summary>
public record GetRecentNewsArticlesQuery(
    int Count = 3,
    bool IncludeArchived = false) : IRequest<Result<IEnumerable<NewsArticleListDto>>>; 