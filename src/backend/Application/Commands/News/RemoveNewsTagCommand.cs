using System;
using MediatR;
using Application.Common;

namespace Application.Commands.News;

/// <summary>
/// Command for removing a tag from a news article
/// </summary>
public record RemoveNewsTagCommand(
    Guid NewsId,
    string Tag) : IRequest<Result<bool>>; 