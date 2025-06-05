using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Queries.News;

/// <summary>
/// Query for retrieving a news article by its ID
/// </summary>
public record GetNewsByIdQuery(Guid NewsId) : IRequest<Result<NewsDto>>; 