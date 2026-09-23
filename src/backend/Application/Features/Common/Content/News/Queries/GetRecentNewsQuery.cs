using MediatR;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Common;
using System.Collections.Generic;

namespace Application.Features.Common.Content.News.Queries;

/// <summary>
/// Query for retrieving recent news articles
/// </summary>
public record GetRecentNewsArticlesQuery(
    int Count = 3,
    bool IncludeArchived = false) : IRequest<Result<IEnumerable<NewsArticleListDto>>>; 
