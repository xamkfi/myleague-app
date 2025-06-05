using System;
using MediatR;
using Application.Common;

namespace Application.Commands.News;

/// <summary>
/// Command for setting/updating an image for a news article
/// </summary>
public record SetNewsImageCommand(
    Guid NewsId,
    string ImageUrl) : IRequest<Result<bool>>; 