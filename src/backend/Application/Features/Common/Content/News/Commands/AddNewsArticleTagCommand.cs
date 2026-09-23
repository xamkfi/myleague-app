using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.Content.News.Commands;

/// <summary>
/// Command for adding a tag to a news article
/// </summary>
public record AddNewsArticleTagCommand(
    Guid NewsId,
    string Tag) : IRequest<Result<bool>>; 
