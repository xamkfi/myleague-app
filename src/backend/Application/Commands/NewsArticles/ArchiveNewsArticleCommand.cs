using System;
using MediatR;
using Application.Common;

namespace Application.Commands.NewsArticles;

/// <summary>
/// Command for archiving a news article
/// </summary>
public record ArchiveNewsArticleCommand(
    Guid Id) : IRequest<Result<bool>>; 