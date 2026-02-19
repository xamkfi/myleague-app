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
/// Command to replace the officials for a match.
/// </summary>
/// <param name="MatchId">Match identifier</param>
/// <param name="OfficialIds">Officials to keep (must be at least one)</param>
public record UpdateMatchOfficialsCommand(Guid MatchId, IReadOnlyCollection<Guid> OfficialIds) : IRequest<Result<FloorballMatchDto>>;

