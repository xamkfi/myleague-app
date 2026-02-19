using Application.Common;
using Application.DTOs.Floorball;
using Domain.Common;
using MediatR;

namespace Application.Queries.Floorball.Match;

/// <summary>
/// Query to get today's matches by team
/// </summary>
public class GetTodaysMatchesByTeamQuery : IRequest<Result<IEnumerable<FloorballMatchDto>>>
{
    /// <summary>
    /// Team's id
    /// </summary>
    public Guid TeamId { get; set; }

    public GetTodaysMatchesByTeamQuery(Guid teamId)
    {
        TeamId = teamId;
    }
} 