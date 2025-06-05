using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.NewsArticles;

/// <summary>
/// Query for retrieving news articles by tag
/// </summary>
public record GetNewsArticlesByTagQuery(
    string Tag) : IRequest<Result<IEnumerable<NewsArticleListDto>>>; 