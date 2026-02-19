using MediatR;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Common;
using System.Collections.Generic;

namespace Application.Features.Common.News.Queries;

/// <summary>
/// Query for retrieving all available news categories
/// </summary>
public record GetNewsArticleCategoriesQuery() : IRequest<Result<IEnumerable<NewsArticleCategoryDto>>>; 
