using MediatR;
using Application.Common;
using System.Collections.Generic;

namespace Application.Features.Common.News.Queries;

/// <summary>
/// Query for retrieving all used tags in news articles
/// </summary>
public record GetNewsArticleTagsQuery() : IRequest<Result<IEnumerable<string>>>; 
