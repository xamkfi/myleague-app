using System;
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
using Domain.Common;
using System.Collections.Generic;

namespace Application.Features.Common.Content.News.Queries;

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
    IReadOnlyCollection<string>? TeamCategories = null,
    string? Tag = null) : IRequest<Result<PagedResult<NewsArticleListDto>>>
{
    /// <summary>
    /// Resource key for pagination configuration
    /// </summary>
    public const string ResourceKey = "News";
} 
