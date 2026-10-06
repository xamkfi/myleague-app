using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Queries;

/// <summary>
/// Gets scheduled matches from <paramref name="From"/> onwards for any of the given teams, earliest first.
/// Draft competitions are excluded.
/// </summary>
public record GetFloorballUpcomingMatchesForTeamsQuery(
    IReadOnlyCollection<Guid> TeamIds,
    DateTime From) : IRequest<Result<IEnumerable<FloorballMatchDto>>>
{
    /// <summary>
    /// Upper bound on returned matches per requested team.
    /// </summary>
    public const int MaxMatchesPerTeam = 100;
}
