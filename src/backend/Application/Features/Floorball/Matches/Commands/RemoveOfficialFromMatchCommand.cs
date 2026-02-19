using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Command for removing an official from a match while enforcing minimum official count.
/// </summary>
/// <param name="MatchId">Match identifier</param>
/// <param name="RefereeId">Referee to remove</param>
public record RemoveOfficialFromMatchCommand(Guid MatchId, Guid RefereeId) : IRequest<Result<FloorballMatchDto>>;

