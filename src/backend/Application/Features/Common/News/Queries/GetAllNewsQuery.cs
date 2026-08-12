using System;
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
using Domain.Common;
using System.Collections.Generic;

namespace Application.Features.Common.News.Queries;

/// <summary>
/// Query for retrieving all news articles with pagination and filtering
/// </summary>
public record GetAllNewsArticlesQuery(
    int Page = 1,
    int PageSize = 0, // 0 means use default from configuration
    string? Category = null,
    string? SportCategory = null,
    string? Search = null,
    string? Author = null,
    bool IncludeArchived = false,
    string? TeamCategory = null) : IRequest<Result<PagedResult<NewsArticleListDto>>>
{
    /// <summary>
    /// Resource key for pagination configuration
    /// </summary>
    public const string ResourceKey = "News";
} 
