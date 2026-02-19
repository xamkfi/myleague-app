using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.News.Commands;

/// <summary>
/// Command for restoring an archived news article
/// </summary>
public record RestoreNewsArticleCommand(
    Guid Id) : IRequest<Result<bool>>; 
