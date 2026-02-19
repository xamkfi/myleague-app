using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.NewsArticles;

/// <summary>
/// Query for retrieving news articles by author
/// </summary>
public record GetNewsArticlesByAuthorQuery(
    string Author) : IRequest<Result<IEnumerable<NewsArticleListDto>>>; 