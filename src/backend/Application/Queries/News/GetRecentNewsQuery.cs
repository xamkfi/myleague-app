using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.News;

/// <summary>
/// Query for retrieving recent news articles
/// </summary>
public record GetRecentNewsQuery(
    int Count = 10,
    bool IncludeArchived = false) : IRequest<Result<IEnumerable<NewsListDto>>>; 