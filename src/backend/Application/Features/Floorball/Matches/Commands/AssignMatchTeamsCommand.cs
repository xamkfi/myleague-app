using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Manually assigns (or clears) the home and away team slots on a scheduled or postponed match.
/// </summary>
/// <remarks>
/// Use-cases:
/// <list type="bullet">
///   <item>Filling in a fixture that was published before the participants were known
///         (e.g. season round 12 announced months in advance).</item>
///   <item>Correcting a playoff bracket slot that was auto-populated from a feeder result the
///         tournament jury has since overruled.</item>
///   <item>Clearing a slot back to "to be determined" — pass <c>null</c> for the relevant team.</item>
/// </list>
/// When this command changes a team on a playoff match, the change is also propagated to the
/// downstream <c>NextMatchId</c> slot (when not yet finished). See
/// <see cref="Handlers.AssignMatchTeamsHandler"/> for the propagation rules.
/// </remarks>
/// <param name="MatchId">The match whose team slots are being updated.</param>
/// <param name="HomeTeamId">The new home team, or <c>null</c> to clear the slot.</param>
/// <param name="AwayTeamId">The new away team, or <c>null</c> to clear the slot.</param>
public record AssignMatchTeamsCommand(
    Guid MatchId,
    Guid? HomeTeamId,
    Guid? AwayTeamId) : IRequest<Result<FloorballMatchDto>>;
