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
            match.Events.OrderBy(e => e.PeriodNumber).ThenBy(e => e.GameTime).Select(ToEventDto).ToList(),
            match.Officials.Select(ToOfficialDto).ToList(),
            match.PeriodScores.OrderBy(p => p.PeriodNumber).Select(ToPeriodScoreDto).ToList());
    }

    public static HockeyMatchTeamDto ToTeamDto(HockeyMatchTeam team)
    {
        IReadOnlyCollection<HockeyMatchActivePlayerDto> players =
            team.PlayerSelection?.ActivePlayers
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
            team.TracksOnIcePlayers,
            team.ActiveGoalieMatchPlayerId,
            players,
            team.Lines.Select(ToLineDto).ToList(),
            team.OnIceState is null ? null : ToOnIceStateDto(team.OnIceState));
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
        Guid? losingActivePlayerId = matchEvent is HockeyFaceoff faceoff
            ? faceoff.LosingActivePlayerId
            : null;

        return new HockeyMatchEventDto(
            matchEvent.Id,
            matchEvent.EventType.ToString(),
            matchEvent.PeriodNumber,
            (int)matchEvent.GameTime.TotalSeconds,
            matchEvent.MatchTeamId,
            matchEvent.MatchActivePlayerId,
            matchEvent.Description,
            losingActivePlayerId);
    }

    public static HockeyMatchOfficialDto ToOfficialDto(HockeyMatchOfficial official)
    {
        return new HockeyMatchOfficialDto(
            official.Id,
            official.OfficialId,
            official.Role.ToString(),
            official.IsMainOfficial);
    }

    public static HockeyPeriodScoreDto ToPeriodScoreDto(HockeyPeriodScore periodScore)
    {
        return new HockeyPeriodScoreDto(
            periodScore.Id,
            periodScore.PeriodNumber,
            periodScore.PeriodType.ToString(),
            periodScore.HomeMatchTeamId,
            periodScore.AwayMatchTeamId,
            periodScore.HomeGoals,
            periodScore.AwayGoals,
            periodScore.IsCompleted);
    }

    public static HockeyMatchLineDto ToLineDto(HockeyMatchLine line)
    {
        return new HockeyMatchLineDto(
            line.Id,
            line.MatchTeamId,
            line.Name,
            line.LineNumber,
            line.LineType.ToString(),
            line.IsActive,
            line.IsLocked,
            line.Notes,
            line.Players.Select(ToLinePlayerDto).ToList());
    }

    public static HockeyMatchLinePlayerDto ToLinePlayerDto(HockeyMatchLinePlayer player)
    {
        return new HockeyMatchLinePlayerDto(
            player.Id,
            player.MatchActivePlayerId,
            player.Slot?.ToString(),
            player.Order);
    }

    public static HockeyOnIceStateDto ToOnIceStateDto(HockeyOnIceState state)
    {
        return new HockeyOnIceStateDto(
            state.Id,
            state.MatchTeamId,
            state.IsEnabled,
            state.Version,
            state.PlayersOnIce.Select(ToOnIcePlayerDto).ToList());
    }

    public static HockeyOnIcePlayerDto ToOnIcePlayerDto(HockeyOnIcePlayer player)
    {
        return new HockeyOnIcePlayerDto(
            player.Id,
            player.MatchActivePlayerId,
            player.Slot?.ToString(),
            player.Order,
            player.IsGoalie,
            player.IsExtraAttacker);
    }
}
