using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;

namespace Application.Features.Football.Matches.Mappings;

/// <summary>
/// Mapper for FootballMatch entity.
/// </summary>
public static class FootballMatchMapper
{
    public static FootballMatchDto ToDto(FootballMatch match)
    {
        return ToDto(match, new Dictionary<Guid, Person>());
    }

    public static FootballMatchDto ToDto(
        FootballMatch match,
        Dictionary<Guid, Person> playerPersonLookup,
        Club? homeClub = null,
        Club? awayClub = null)
    {
        ArgumentNullException.ThrowIfNull(match);
        playerPersonLookup ??= new Dictionary<Guid, Person>();

        List<Guid> officials = match.Officials.Select(referee => referee.Id).ToList();

        Dictionary<int, FootballPeriodScoreDto> periodScores = match.PeriodScores
            .OrderBy(ps => ps.PeriodNumber)
            .ToDictionary(
                ps => ps.PeriodNumber,
                ps => new FootballPeriodScoreDto(ps.HomeScore, ps.AwayScore, ps.IsCompleted));

        List<FootballGoalEventDto> goalEvents = match.GoalEvents
            .Select(g => new FootballGoalEventDto(
                g.Id,
                g.TeamId,
                g.ScoringPlayerId,
                g.AssistingPlayerId,
                g.PeriodNumber,
                g.TimeInSeconds,
                GetPlayerName(g.ScoringPlayerId, playerPersonLookup),
                g.AssistingPlayerId.HasValue ? GetPlayerName(g.AssistingPlayerId, playerPersonLookup) : null,
                g.GoalType,
                g.Description))
            .ToList();

        List<FootballCardEventDto> cardEvents = match.CardEvents
            .Select(c => new FootballCardEventDto(
                c.Id,
                c.TeamId,
                c.PlayerId,
                c.CardType,
                c.PeriodNumber,
                c.TimeInSeconds,
                GetPlayerName(c.PlayerId, playerPersonLookup),
                c.Description))
            .ToList();

        List<FootballSubstitutionEventDto> substitutionEvents = match.SubstitutionEvents
            .Select(s => new FootballSubstitutionEventDto(
                s.Id,
                s.TeamId,
                s.PlayerOffId,
                s.PlayerOnId,
                s.PeriodNumber,
                s.TimeInSeconds,
                GetPlayerName(s.PlayerOffId, playerPersonLookup),
                GetPlayerName(s.PlayerOnId, playerPersonLookup),
                s.Description))
            .ToList();

        Uri? homeTeamLogo = match.HomeTeam?.GetEffectiveLogoUrl(homeClub?.LogoUrl);
        Uri? awayTeamLogo = match.AwayTeam?.GetEffectiveLogoUrl(awayClub?.LogoUrl);

        Guid? homeTeamId = match.HomeTeamId;
        Guid? awayTeamId = match.AwayTeamId;

        List<FootballLineupPlayerDto> homeLineup = homeTeamId is Guid homeId
            ? match.Lineup
                .Where(p => p.TeamId == homeId)
                .Select(p => new FootballLineupPlayerDto(p.PlayerId, p.Position, p.IsOnField, p.IsSentOff))
                .ToList()
            : new List<FootballLineupPlayerDto>();

        List<FootballLineupPlayerDto> awayLineup = awayTeamId is Guid awayId
            ? match.Lineup
                .Where(p => p.TeamId == awayId)
                .Select(p => new FootballLineupPlayerDto(p.PlayerId, p.Position, p.IsOnField, p.IsSentOff))
                .ToList()
            : new List<FootballLineupPlayerDto>();

        return new FootballMatchDto(
            match.Id,
            match.CompetitionId,
            match.Competition?.Name ?? string.Empty,
            match.HomeTeamId,
            match.HomeTeam?.Name,
            homeTeamLogo,
            match.AwayTeamId,
            match.AwayTeam?.Name,
            awayTeamLogo,
            match.ScheduledDateTime.ToUniversalTime(),
            match.Venue,
            match.Status,
            match.HomeScore,
            match.AwayScore,
            match.WentToExtraTime,
            match.WentToPenaltyShootout,
            periodScores,
            officials,
            goalEvents,
            cardEvents,
            substitutionEvents,
            MapMatchRules(match.MatchRules),
            homeLineup,
            awayLineup,
            match.TournamentGroupId,
            match.TournamentStage?.ToString(),
            ResolveCompetitionType(match),
            match.PlayoffRound,
            match.PlayoffMatchOrder,
            match.NextMatchId,
            match.NextMatchSlot);
    }

    public static IEnumerable<FootballMatchDto> ToDtos(IEnumerable<FootballMatch> matches)
    {
        ArgumentNullException.ThrowIfNull(matches);
        return matches.Select(match => ToDto(match));
    }

    public static IEnumerable<FootballMatchDto> ToDtos(IEnumerable<FootballMatch> matches, Dictionary<Guid, Club> clubLookup)
    {
        ArgumentNullException.ThrowIfNull(matches);
        clubLookup ??= new Dictionary<Guid, Club>();

        return matches.Select(match =>
        {
            Club? homeClub = null;
            Club? awayClub = null;
            if (match.HomeTeam != null)
            {
                clubLookup.TryGetValue(match.HomeTeam.ClubId, out homeClub);
            }

            if (match.AwayTeam != null)
            {
                clubLookup.TryGetValue(match.AwayTeam.ClubId, out awayClub);
            }

            return ToDto(match, new Dictionary<Guid, Person>(), homeClub, awayClub);
        });
    }

    public static FootballMatch ToEntity(
        CreateFootballMatchCommand command,
        FootballCompetition competition,
        FootballTeam? homeTeam,
        FootballTeam? awayTeam,
        FootballReferee? referee = null)
    {
        ArgumentNullException.ThrowIfNull(command);

        DateTime scheduledDateTimeUtc = command.ScheduledDateTime.Kind switch
        {
            DateTimeKind.Utc => command.ScheduledDateTime,
            DateTimeKind.Local => command.ScheduledDateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(command.ScheduledDateTime, DateTimeKind.Utc)
        };

        FootballMatch match = new(
            competition,
            homeTeam,
            awayTeam,
            scheduledDateTimeUtc,
            command.Venue);

        if (referee != null)
        {
            match.AddOfficial(referee);
        }

        if (command.TournamentStage.HasValue || command.TournamentGroupId.HasValue)
        {
            FootballTournamentStage stage = command.TournamentStage ?? FootballTournamentStage.GroupStage;
            match.SetTournamentInfo(stage, command.TournamentGroupId);
        }

        return match;
    }

    public static void UpdateFromCommand(FootballMatch match, UpdateFootballMatchCommand command)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(command);
        match.Reschedule(command.ScheduledDateTime, command.Venue);
    }

    private static FootballMatchRulesDto MapMatchRules(FootballMatchRules rules) =>
        new(
            rules.NumberOfHalves,
            rules.HalfDurationMinutes,
            rules.PlayersOnField,
            rules.RequireGoalkeeper,
            rules.MaxSubstitutions,
            rules.RequireOfficialsToStart,
            rules.AllowExtraTime,
            rules.ExtraTimeHalfCount,
            rules.ExtraTimeHalfDurationMinutes,
            rules.AllowPenaltyShootout);

    private static string GetPlayerName(Guid? playerId, Dictionary<Guid, Person> playerPersonLookup)
    {
        if (!playerId.HasValue || !playerPersonLookup.TryGetValue(playerId.Value, out Person? person))
        {
            return "Unknown Player";
        }

        return $"{person.FirstName} {person.LastName}";
    }

    private static FootballCompetitionType ResolveCompetitionType(FootballMatch match)
    {
        if (match.Competition is FootballTournament)
        {
            return FootballCompetitionType.Tournament;
        }

        if (match.Competition is FootballSeason)
        {
            return FootballCompetitionType.Season;
        }

        if (match.TournamentGroupId.HasValue || match.TournamentStage.HasValue)
        {
            return FootballCompetitionType.Tournament;
        }

        return FootballCompetitionType.Season;
    }
}
