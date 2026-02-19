using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.NewsArticles;

/// <summary>
/// Query for searching news articles by search term
/// </summary>
public record SearchNewsArticlesQuery(
    string SearchTerm) : IRequest<Result<IEnumerable<NewsArticleListDto>>>; 