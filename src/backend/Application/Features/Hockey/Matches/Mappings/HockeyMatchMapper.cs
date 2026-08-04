using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;

namespace Application.Features.Hockey.Matches.Mappings;

/// <summary>
/// Maps hockey match domain entities to application DTOs.
/// </summary>
public static class HockeyMatchMapper
{
    public static HockeyMatchDto ToDto(HockeyMatch match)
    {
        return new HockeyMatchDto(
            match.Id,
            match.CompetitionId,
            match.CompetitionDivisionId,
            match.TournamentGroupId,
            match.PlayoffSeriesId,
            match.ScheduledStartTime,
            match.ActualStartTime,
            match.ActualEndTime,
            match.Venue,
            match.MatchType.ToString(),
            match.Status.ToString(),
            match.ResultType?.ToString(),
            match.CurrentPeriodNumber,
            match.WentToOvertime,
            match.WentToShootout,
            match.HomeTeamId,
            match.AwayTeamId,
            match.HomeScore,
            match.AwayScore,
            match.MatchTeams.Select(ToTeamDto).ToList(),
            match.Events.OrderBy(e => e.PeriodNumber).ThenBy(e => e.GameTime).Select(ToEventDto).ToList());
    }

    public static HockeyMatchTeamDto ToTeamDto(HockeyMatchTeam team)
    {
        IReadOnlyCollection<HockeyMatchActivePlayerDto> players =
            team.PlayerSelection?.ActivePlayers
                .Where(p => p.IsActive)
                .Select(ToActivePlayerDto)
                .ToList()
            ?? (IReadOnlyCollection<HockeyMatchActivePlayerDto>)Array.Empty<HockeyMatchActivePlayerDto>();

        return new HockeyMatchTeamDto(
            team.Id,
            team.MatchId,
            team.TeamId,
            team.CompetitionTeamId,
            team.TeamSlot.ToString(),
            team.Goals,
            team.PlayerSelection?.IsConfirmed ?? false,
            players);
    }

    public static HockeyMatchActivePlayerDto ToActivePlayerDto(HockeyMatchActivePlayer player)
    {
        return new HockeyMatchActivePlayerDto(
            player.Id,
            player.TeamPlayerId,
            player.JerseyNumber,
            player.Position.ToString(),
            player.IsActive,
            player.IsStartingPlayer,
            player.IsGoalie);
    }

    public static HockeyMatchEventDto ToEventDto(HockeyMatchEvent matchEvent)
    {
        return new HockeyMatchEventDto(
            matchEvent.Id,
            matchEvent.EventType.ToString(),
            matchEvent.PeriodNumber,
            (int)matchEvent.GameTime.TotalSeconds,
            matchEvent.MatchTeamId,
            matchEvent.MatchActivePlayerId,
            matchEvent.Description);
    }
}
