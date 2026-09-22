using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;

namespace Application.Features.Hockey.Matches.Mappings;

/// <summary>
/// Maps hockey matches to the public list DTO using a batch-loaded team-name lookup.
/// </summary>
public static class HockeyMatchListMapper
{
    public static HockeyMatchListDto ToDto(
        HockeyMatch match,
        IReadOnlyDictionary<Guid, string> teamNames)
    {
        Guid? homeTeamId = match.HomeTeamId;
        Guid? awayTeamId = match.AwayTeamId;

        return new HockeyMatchListDto(
            match.Id,
            match.ScheduledStartTime,
            match.Status.ToString(),
            match.MatchType.ToString(),
            match.Venue,
            match.CompetitionId,
            match.Competition?.Name,
            homeTeamId,
            awayTeamId,
            ResolveTeamName(homeTeamId, teamNames),
            ResolveTeamName(awayTeamId, teamNames),
            match.HomeScore,
            match.AwayScore);
    }

    public static IReadOnlyList<HockeyMatchListDto> ToDtos(
        IEnumerable<HockeyMatch> matches,
        IReadOnlyDictionary<Guid, string> teamNames)
    {
        return matches.Select(match => ToDto(match, teamNames)).ToList();
    }

    private static string? ResolveTeamName(
        Guid? teamId,
        IReadOnlyDictionary<Guid, string> teamNames)
    {
        if (teamId is Guid id && teamNames.TryGetValue(id, out string? name))
        {
            return name;
        }

        return null;
    }
}
