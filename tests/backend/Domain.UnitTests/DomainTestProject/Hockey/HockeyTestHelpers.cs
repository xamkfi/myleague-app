using System.Reflection;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;
using Domain.ValueObjects.Hockey.Rules;

namespace DomainTestProject.Hockey;

/// <summary>
/// Shared factories for hockey domain unit tests (real entities, no EF).
/// </summary>
internal static class HockeyTestHelpers
{
    public static Club CreateClub(string name = "Test HC") => new(name);

    public static HockeyTeam CreateTeam(string name = "Wolves")
    {
        Club club = CreateClub(name + " Club");
        return club.AddHockeyTeam(name, TeamCategory.Adult, homeArena: "Test Arena", primaryJerseyColor: "Blue");
    }

    public static HockeyPlayer CreatePlayer(HockeyPosition position = HockeyPosition.Center)
    {
        Person person = new("Test", "Player");
        return new HockeyPlayer(person.Id, position);
    }

    public static HockeySeason CreateSeason(string name = "2026-2027")
    {
        return new HockeySeason(
            name,
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            seasonCode: "26-27");
    }

    public static HockeyTournament CreateTournament(string name = "Cup")
    {
        return new HockeyTournament(
            name,
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc),
            venue: "Arena");
    }

    public static void ActivateCompetition(HockeyCompetition competition)
    {
        competition.Publish();
        competition.Activate();
    }

    public static HockeyMatch CreateStandaloneMatch()
    {
        return new HockeyMatch(
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.Friendly);
    }

    public static HockeyMatch CreateCompetitionMatch(HockeyCompetition competition)
    {
        return new HockeyMatch(
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.League,
            competitionId: competition.Id);
    }

    public static (HockeyMatch Match, HockeyMatchTeam Home, HockeyMatchTeam Away) CreateMatchWithSides(
        HockeyTeam homeTeam,
        HockeyTeam awayTeam,
        HockeyCompetition? competition = null,
        HockeyCompetitionTeam? homeCompetitionTeam = null,
        HockeyCompetitionTeam? awayCompetitionTeam = null)
    {
        HockeyMatch match = competition is null
            ? CreateStandaloneMatch()
            : CreateCompetitionMatch(competition);

        HockeyMatchTeam home = match.AssignMatchTeam(
            homeTeam.Id,
            HockeyTeamSlot.Home,
            homeCompetitionTeam);
        HockeyMatchTeam away = match.AssignMatchTeam(
            awayTeam.Id,
            HockeyTeamSlot.Away,
            awayCompetitionTeam);
        return (match, home, away);
    }

    public static HockeyTeamPlayer AddRosterPlayer(
        HockeyTeam team,
        HockeyPlayer? player = null,
        HockeyPosition position = HockeyPosition.Center,
        int? jerseyNumber = 10,
        Guid? competitionId = null,
        HockeyCaptainRole captainRole = HockeyCaptainRole.None,
        HockeyRosterRules? rules = null)
    {
        player ??= CreatePlayer(position);
        HockeyTeamPlayer teamPlayer = team.AddPlayer(player, position, competitionId, jerseyNumber, rosterRules: rules);
        if (captainRole != HockeyCaptainRole.None)
        {
            team.UpdateTeamPlayer(
                player.Id,
                position,
                jerseyNumber,
                HockeyRosterStatus.Active,
                captainRole,
                competitionId,
                rules);
        }

        return teamPlayer;
    }

    public static HockeyMatchActivePlayer DressPlayer(
        HockeyMatchTeam matchTeam,
        HockeyTeamPlayer teamPlayer,
        int jerseyNumber,
        bool isGoalie = false,
        bool isStartingPlayer = false)
    {
        HockeyMatchPlayerSelection selection = matchTeam.PlayerSelection
            ?? matchTeam.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer active = selection.AddActivePlayer(
            teamPlayer,
            jerseyNumber,
            isGoalie: isGoalie,
            isStartingPlayer: isStartingPlayer);
        AttachTeamPlayer(active, teamPlayer);
        return active;
    }

    public static HockeyCoachChallengeRules FailedChallengeBenchMinorRules() =>
        new(
            enabled: true,
            maxChallengesPerTeam: 1,
            loseChallengeAfterFailed: true,
            penaltyForFailedChallenge: true,
            failedChallengePenaltyMinutes: 2,
            failedChallengePenaltyOffence: HockeyPenaltyOffence.DelayOfGame,
            failedChallengePenaltySeverity: HockeyPenaltySeverity.BenchMinor,
            allowChallengeInOvertime: true,
            allowChallengeInShootout: false);

    /// <summary>
    /// Sets the TeamPlayer navigation for statistics calculation tests (EF would load this).
    /// </summary>
    public static void AttachTeamPlayer(HockeyMatchActivePlayer active, HockeyTeamPlayer teamPlayer)
    {
        PropertyInfo? property = typeof(HockeyMatchActivePlayer)
            .GetProperty(nameof(HockeyMatchActivePlayer.TeamPlayer));
        property!.SetValue(active, teamPlayer);
    }
}
