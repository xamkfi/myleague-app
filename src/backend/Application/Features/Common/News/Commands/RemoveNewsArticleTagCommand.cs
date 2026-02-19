using System;
using MediatR;
using Application.Common;

namespace Application.Commands.NewsArticles;

/// <summary>
/// Command for removing a tag from a news article
/// </summary>
public record RemoveNewsArticleTagCommand(
    Guid NewsId,
    string Tag) : IRequest<Result<bool>>; 