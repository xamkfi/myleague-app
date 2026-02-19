using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.NewsArticles;

/// <summary>
/// Query for retrieving all available news categories
/// </summary>
public record GetNewsArticleCategoriesQuery() : IRequest<Result<IEnumerable<NewsArticleCategoryDto>>>; 