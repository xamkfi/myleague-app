using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Queries;

/// <summary>
/// Gets scheduled matches from <paramref name="From"/> onwards for any of the given teams, earliest first.
/// Draft competitions are excluded.
/// </summary>
public record GetFootballUpcomingMatchesForTeamsQuery(
    IReadOnlyCollection<Guid> TeamIds,
    DateTime From) : IRequest<Result<IEnumerable<FootballMatchDto>>>
{
    /// <summary>
    /// Upper bound on returned matches per requested team.
    /// </summary>
    public const int MaxMatchesPerTeam = 100;
}
