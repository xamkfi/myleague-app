using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.NewsArticles;

/// <summary>
/// Query for retrieving news articles by category
/// </summary>
public record GetNewsArticlesByCategoryQuery(
    string Category) : IRequest<Result<IEnumerable<NewsArticleListDto>>>; 