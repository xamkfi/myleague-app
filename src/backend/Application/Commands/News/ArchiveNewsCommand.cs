using System;
using MediatR;
using Application.Common;

namespace Application.Commands.News;

/// <summary>
/// Command for archiving a news article
/// </summary>
public record ArchiveNewsCommand(
    Guid Id) : IRequest<Result<bool>>; 