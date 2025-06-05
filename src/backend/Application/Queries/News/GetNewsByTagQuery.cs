using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.News;

/// <summary>
/// Query for retrieving news articles by tag
/// </summary>
public record GetNewsByTagQuery(
    string Tag) : IRequest<Result<IEnumerable<NewsListDto>>>; 