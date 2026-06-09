using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Command for appending a single official (referee) to an existing floorball match. Has
/// append semantics — if the referee is already attached the match is returned unchanged.
/// </summary>
/// <param name="MatchId">Target match.</param>
/// <param name="RefereeId">Referee to append.</param>
public record AddOfficialToMatchCommand(Guid MatchId, Guid RefereeId) : IRequest<Result<FloorballMatchDto>>;
