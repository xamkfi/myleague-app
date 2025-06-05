using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.News;

/// <summary>
/// Query for retrieving news articles by category
/// </summary>
public record GetNewsByCategoryQuery(
    string Category) : IRequest<Result<IEnumerable<NewsListDto>>>; 