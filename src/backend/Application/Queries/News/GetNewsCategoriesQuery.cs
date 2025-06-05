using MediatR;
using Application.DTOs.Common;
using Application.Common;
using System.Collections.Generic;

namespace Application.Queries.News;

/// <summary>
/// Query for retrieving all available news categories
/// </summary>
public record GetNewsCategoriesQuery() : IRequest<Result<IEnumerable<NewsCategoryDto>>>; 