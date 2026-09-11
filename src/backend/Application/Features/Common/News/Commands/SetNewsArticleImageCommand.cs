using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.News.Commands;

/// <summary>
/// Command for setting/updating an image for a news article
/// </summary>
public record SetNewsArticleImageCommand(
    Guid NewsId,
    string ImageUrl) : IRequest<Result<bool>>; 
