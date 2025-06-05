using System;
using MediatR;
using Application.Common;

namespace Application.Commands.News;

/// <summary>
/// Command for adding a tag to a news article
/// </summary>
public record AddNewsTagCommand(
    Guid NewsId,
    string Tag) : IRequest<Result<bool>>; 