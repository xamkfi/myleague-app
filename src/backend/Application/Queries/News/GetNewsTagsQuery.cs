using MediatR;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.News;

/// <summary>
/// Query for retrieving all used tags in news articles
/// </summary>
public record GetNewsTagsQuery() : IRequest<Result<IEnumerable<string>>>; 