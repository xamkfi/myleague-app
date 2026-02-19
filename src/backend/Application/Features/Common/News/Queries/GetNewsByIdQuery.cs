using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Queries.NewsArticles;

/// <summary>
/// Query for retrieving a news article by its ID
/// </summary>
public record GetNewsArticleByIdQuery(Guid NewsId) : IRequest<Result<NewsArticleDto>>; 