using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.News;

/// <summary>
/// Query for searching news articles by search term
/// </summary>
public record SearchNewsQuery(
    string SearchTerm) : IRequest<Result<IEnumerable<NewsListDto>>>; 