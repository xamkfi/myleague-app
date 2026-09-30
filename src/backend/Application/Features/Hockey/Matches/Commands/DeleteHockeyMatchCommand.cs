using Application.Common;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Deletes a hockey match that is still scheduled.
/// </summary>
public record DeleteHockeyMatchCommand(Guid MatchId) : IRequest<Result>;
