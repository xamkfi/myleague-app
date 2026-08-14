using Application.Common;
using Application.Features.Common.TeamLeader.DTOs;
using MediatR;

namespace Application.Features.Common.TeamLeader.Queries;

/// <summary>
/// Query for retrieving all teams (both sports) that the given person actively manages.
/// </summary>
/// <param name="PersonId">The person ID of the team leader</param>
public record GetMyTeamsQuery(Guid PersonId) : IRequest<Result<IEnumerable<TeamLeaderTeamDto>>>;
