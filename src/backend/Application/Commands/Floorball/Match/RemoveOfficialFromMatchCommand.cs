using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match;

/// <summary>
/// Command for removing an official from a match while enforcing minimum official count.
/// </summary>
/// <param name="MatchId">Match identifier</param>
/// <param name="RefereeId">Referee to remove</param>
public record RemoveOfficialFromMatchCommand(Guid MatchId, Guid RefereeId) : IRequest<Result<FloorballMatchDto>>;

