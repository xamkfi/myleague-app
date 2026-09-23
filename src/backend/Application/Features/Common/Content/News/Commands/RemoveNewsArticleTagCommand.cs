using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.Content.News.Commands;

/// <summary>
/// Command for removing a tag from a news article
/// </summary>
public record RemoveNewsArticleTagCommand(
    Guid NewsId,
    string Tag) : IRequest<Result<bool>>; 
