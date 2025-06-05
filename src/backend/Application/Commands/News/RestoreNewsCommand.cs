using System;
using MediatR;
using Application.Common;

namespace Application.Commands.News;

/// <summary>
/// Command for restoring an archived news article
/// </summary>
public record RestoreNewsCommand(
    Guid Id) : IRequest<Result<bool>>; 