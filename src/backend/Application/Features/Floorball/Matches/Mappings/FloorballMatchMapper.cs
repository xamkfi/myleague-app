using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.ValueObjects.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Floorball.Matches.Mappings;

/// <summary>
/// Mapper for FloorballMatch entity
/// </summary>
public static class FloorballMatchMapper
{
    /// <summary>
    /// Maps a FloorballMatch entity to a FloorballMatchDto
    /// </summary>
    /// <param name="match">The match entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when match is null</exception>
    public static FloorballMatchDto ToDto(FloorballMatch match)
    {
        return ToDto(match, new Dictionary<Guid, Person>());
    }

    /// <summary>
    /// Maps a FloorballMatch entity to a FloorballMatchDto with person lookup for player names
    /// </summary>
    /// <param name="match">The match entity to map</param>
    /// <param name="playerPersonLookup">Dictionary mapping player IDs to their person data</param>
    /// <param name="homeClub">Optional club entity for the home team (used for logo fallback)</param>
    /// <param name="awayClub">Optional club entity for the away team (used for logo fallback)</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when match is null</exception>
    public static FloorballMatchDto ToDto(FloorballMatch match, Dictionary<Guid, Person> playerPersonLookup, Club? homeClub = null, Club? awayClub = null)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        playerPersonLookup ??= new Dictionary<Guid, Person>();

        // Map officials from the match entity
        List<Guid> officials = match.Officials.Select(referee => referee.Id).ToList();

        // Map period scores from the match entity, ordered by period number
        Dictionary<int, PeriodScoreDto> periodScores = match.PeriodScores
            .OrderBy(ps => ps.PeriodNumber)
            .ToDictionary(
                ps => ps.PeriodNumber,
                ps => new PeriodScoreDto(ps.HomeScore, ps.AwayScore, ps.IsCompleted)
            );

        // Map goal events with player names
        List<FloorballGoalEventDto> goalEvents = match.GoalEvents
            .Select(g => new FloorballGoalEventDto(
                g.Id,
                g.TeamId,
                g.ScoringPlayerId ?? Guid.Empty,
                g.AssistingPlayerId,
                g.SecondaryAssistingPlayerId,
                g.PeriodNumber,
                g.TimeInSeconds,
                match.WentToOvertime,
                match.WentToShootout,
                GetPlayerName(g.ScoringPlayerId, playerPersonLookup),
                GetPlayerName(g.AssistingPlayerId, playerPersonLookup),
                GetPlayerName(g.SecondaryAssistingPlayerId, playerPersonLookup)))
            .ToList();

        // Map penalty events with player names
        List<FloorballPenaltyEventDto> penaltyEvents = match.PenaltyEvents
            .Select(p => new FloorballPenaltyEventDto(
                p.Id,
                p.TeamId,
                p.PlayerId,
                p.PenaltyType,
                p.DurationInMinutes,
                p.PeriodNumber,
                p.TimeInSeconds,
                p.Description ?? string.Empty,
                GetPlayerName(p.PlayerId, playerPersonLookup)))
            .ToList();

        // Map save events with goalie names
        List<FloorballSaveEventDto> saveEvents = match.SaveEvents
            .Select(s => new FloorballSaveEventDto
            {
                Id = s.Id,
                TeamId = s.TeamId,
                GoalieId = s.GoalieId,
                PeriodNumber = s.PeriodNumber,
                TimeInSeconds = s.TimeInSeconds,
                WasInOvertime = s.WasInOvertime,
                WasInShootout = s.WasInShootout,
                GoalieName = GetPlayerName(s.GoalieId, playerPersonLookup)
            })
            .ToList();

        // Resolve logos with club fallback
        Uri? homeTeamLogo = match.HomeTeam.GetEffectiveLogoUrl(homeClub?.LogoUrl);
        Uri? awayTeamLogo = match.AwayTeam.GetEffectiveLogoUrl(awayClub?.LogoUrl);

        FloorballMatchRulesDto matchRulesDto = MapMatchRules(match.MatchRules);

        return new FloorballMatchDto(
            match.Id,
            match.CompetitionId,
            match.Competition.Name,
            match.HomeTeamId,
            match.HomeTeam.Name,
            homeTeamLogo,
            match.AwayTeamId,
            match.AwayTeam.Name,
            awayTeamLogo,
            match.ScheduledDateTime.ToUniversalTime(),
            match.Venue,
            match.Status,
            match.HomeScore,
            match.AwayScore,
            match.WentToOvertime,
            match.WentToShootout,
            match.HomeActiveGoalieId,
            match.AwayActiveGoalieId,
            periodScores,
            officials,
            goalEvents,
            penaltyEvents,
            saveEvents,
            matchRulesDto,
            match.TournamentGroupId,
            match.TournamentStage?.ToString());
    }

    /// <summary>
    /// Maps a collection of FloorballMatch entities to FloorballMatchDtos
    /// </summary>
    /// <param name="matches">The match entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when matches is null</exception>
    public static IEnumerable<FloorballMatchDto> ToDtos(IEnumerable<FloorballMatch> matches)
    {
        if (matches == null)
            throw new ArgumentNullException(nameof(matches));

        return matches.Select(match => ToDto(match));
    }

    /// <summary>
    /// Maps a collection of FloorballMatch entities to FloorballMatchDto objects with club data
    /// </summary>
    /// <param name="matches">The matches to map</param>
    /// <param name="clubLookup">Dictionary mapping club IDs to club entities</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when matches is null</exception>
    public static IEnumerable<FloorballMatchDto> ToDtos(IEnumerable<FloorballMatch> matches, Dictionary<Guid, Club> clubLookup)
    {
        if (matches == null)
            throw new ArgumentNullException(nameof(matches));

        clubLookup ??= new Dictionary<Guid, Club>();

        return matches.Select(match => 
        {
            clubLookup.TryGetValue(match.HomeTeam.ClubId, out Club? homeClub);
            clubLookup.TryGetValue(match.AwayTeam.ClubId, out Club? awayClub);

            return ToDto(match, new Dictionary<Guid, Person>(), homeClub, awayClub);
        });
    }

    /// <summary>
    /// Maps an EventSourcedFloorballMatch entity to a FloorballMatchDto
    /// </summary>
    /// <param name="match">The event-sourced match entity to map</param>
    /// <param name="homeTeamName">The home team name (placeholder until team lookups are implemented)</param>
    /// <param name="awayTeamName">The away team name (placeholder until team lookups are implemented)</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when match is null</exception>
    public static FloorballMatchDto ToDto(FloorballMatch match, string homeTeamName = "Home Team", string awayTeamName = "Away Team")
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        // Convert period scores from tuple format to PeriodScoreDto format
        Dictionary<int, PeriodScoreDto> periodScores = ConvertPeriodScores(match.PeriodScores);
        List<Guid> officials = ConvertOfficials(match.Officials);

        FloorballMatchRulesDto matchRulesDto = MapMatchRules(match.MatchRules);

        return new FloorballMatchDto(
            match.Id,
            match.CompetitionId,
            "default",
            match.HomeTeamId,
            homeTeamName,
            null,
            match.AwayTeamId,
            awayTeamName,
            null,
            match.ScheduledDateTime,
            match.Venue,
            match.Status,
            match.HomeScore,
            match.AwayScore,
            match.WentToOvertime,
            match.WentToShootout,
            null, // EventSourcedFloorballMatch does not have HomeActiveGoalieId
            null, // EventSourcedFloorballMatch does not have AwayActiveGoalieId
            periodScores,
            officials,
            new List<FloorballGoalEventDto>(), // TODO: Map goal events when needed
            new List<FloorballPenaltyEventDto>(), // TODO: Map penalty events when needed
            new List<FloorballSaveEventDto>(),
            matchRulesDto,
            match.TournamentGroupId,
            match.TournamentStage?.ToString()
        );
    }

    /// <summary>
    /// Creates a new FloorballMatch entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <param name="season">The season entity</param>
    /// <param name="homeTeam">The home team entity</param>
    /// <param name="awayTeam">The away team entity</param>
    /// <param name="referee">The referee entity (optional)</param>
    /// <returns>The new match entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    /// <exception cref="NotSupportedException">Thrown because FloorballMatch creation requires loaded entities</exception>
    public static FloorballMatch ToEntity(CreateFloorballMatchCommand command, FloorballCompetition competition, FloorballTeam homeTeam, FloorballTeam awayTeam, FloorballReferee? referee = null)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone
        DateTime scheduledDateTimeUtc = command.ScheduledDateTime.Kind switch
        {
            DateTimeKind.Utc => command.ScheduledDateTime,
            DateTimeKind.Local => command.ScheduledDateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.ScheduledDateTime, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.ScheduledDateTime, DateTimeKind.Utc)
        };

        FloorballMatch match = new FloorballMatch(
            competition,
            homeTeam,
            awayTeam,
            scheduledDateTimeUtc,
            command.Venue
            );

        // Add referee if provided
        if (referee != null)
        {
            match.AddOfficial(referee);
        }

        return match;
    }

    /// <summary>
    /// Updates a FloorballMatch entity from an update command
    /// </summary>
    /// <param name="match">The match entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when match or command is null</exception>
    public static void UpdateFromCommand(FloorballMatch match, UpdateFloorballMatchCommand command)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        
        // Use the domain entity's Reschedule method to update scheduled date/time and venue
        // This properly handles business rules and domain events
        match.Reschedule(command.ScheduledDateTime, command.Venue);
    }

    /// <summary>
    /// Converts period scores from tuple format to PeriodScoreDto format
    /// </summary>
    /// <param name="periodScores">The period scores in tuple format</param>
    /// <returns>The period scores in PeriodScoreDto format</returns>
    public static Dictionary<int, PeriodScoreDto> ConvertPeriodScores(IReadOnlyCollection<FloorballPeriodScore> periodScores)
    {
        if (periodScores == null)
            return new Dictionary<int, PeriodScoreDto>();


        return periodScores.ToDictionary(
            kvp => kvp.PeriodNumber,
            kvp => new PeriodScoreDto(kvp.HomeScore, kvp.AwayScore, kvp.IsCompleted)
        );
    }

    //public static Dictionary<int, PeriodScoreDto> ConvertPeriodScores(IReadOnlyDictionary<int, (int HomeScore, int AwayScore)> periodScores)
    //{
    //    if (periodScores == null)
    //        return new Dictionary<int, PeriodScoreDto>();

    //    return periodScores.ToDictionary(
    //        kvp => kvp.Key,
    //        kvp => new PeriodScoreDto(kvp.Value.HomeScore, kvp.Value.AwayScore)
    //    );
    //}

    public static List<Guid> ConvertOfficials(IReadOnlyCollection<FloorballReferee> referees)
    {
        return referees.Select(r => r.Id).ToList();
    }



    /// <summary>
    /// Maps a FloorballMatchRules value object to a FloorballMatchRulesDto
    /// </summary>
    /// <param name="rules">The match rules value object</param>
    /// <returns>The mapped DTO</returns>
    private static FloorballMatchRulesDto MapMatchRules(FloorballMatchRules rules)
    {
        return new FloorballMatchRulesDto(
            rules.NumberOfPeriods,
            rules.PeriodDurationMinutes,
            rules.AllowOvertime,
            rules.OvertimeDurationMinutes,
            rules.AllowShootout);
    }

    /// <summary>
    /// Gets the player name from the person lookup dictionary
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="playerPersonLookup">Dictionary mapping player IDs to their person data</param>
    /// <returns>The player's full name or "Unknown Player" if not found</returns>
    private static string GetPlayerName(Guid? playerId, Dictionary<Guid, Person> playerPersonLookup)
    {
        if (!playerId.HasValue || !playerPersonLookup.TryGetValue(playerId.Value, out Person? person))
        {
            return "Unknown Player";
        }

        return $"{person.FirstName} {person.LastName}";
    }
} 
