using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.News.Commands;

/// <summary>
/// Command for archiving a news article
/// </summary>
public record ArchiveNewsArticleCommand(
    Guid Id) : IRequest<Result<bool>>; 
