using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;

namespace DomainTestProject;

/// <summary>
/// Domain tests for hobby football match rules, lineup, goals, cards, substitutions, and completion.
/// </summary>
public class FootballMatchTests
{
    private static FootballMatchRules FiveASideRules(
        int maxSubstitutions = 0,
        bool requireOfficials = false,
        bool allowExtraTime = false,
        bool allowPenaltyShootout = false) =>
        new(
            numberOfHalves: 2,
            halfDurationMinutes: 20,
            playersOnField: 5,
            requireGoalkeeper: true,
            maxSubstitutions: maxSubstitutions,
            requireOfficialsToStart: requireOfficials,
            allowExtraTime: allowExtraTime,
            extraTimeHalfCount: 2,
            extraTimeHalfDurationMinutes: 5,
            allowPenaltyShootout: allowPenaltyShootout);

    private static FootballSeason CreateSeason(FootballMatchRules? rules = null) =>
        new(
            "Hobby Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            rules ?? FiveASideRules());

    private static (FootballTeam Team, List<FootballPlayer> Players) CreateTeamWithSquad(string name, int fieldPlayers)
    {
        Club club = new Club(name + " FC");
        FootballTeam team = new(
            name,
            divisionId: null,
            club,
            homeArena: "Test Pitch",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);

        List<FootballPlayer> players = new();
        FootballPlayer goalkeeper = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Goalkeeper));
        team.AddPlayer(goalkeeper, FootballPosition.Goalkeeper, jerseyNumber: 1);
        players.Add(goalkeeper);

        for (int i = 0; i < fieldPlayers; i++)
        {
            FootballPlayer player = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));
            team.AddPlayer(player, FootballPosition.Midfielder, jerseyNumber: i + 2);
            players.Add(player);
        }

        return (team, players);
    }

    private static void SetStartingLineup(FootballMatch match, FootballTeam team, IReadOnlyList<FootballPlayer> players, int onFieldCount)
    {
        List<FootballLineupSelection> selections = new();
        for (int i = 0; i < players.Count; i++)
        {
            FootballPosition position = i == 0 ? FootballPosition.Goalkeeper : FootballPosition.Midfielder;
            selections.Add(new FootballLineupSelection(players[i].Id, position, i < onFieldCount));
        }
        match.SetLineup(team.Id, selections);
    }

    private sealed record ReadyMatch(
        FootballMatch Match,
        FootballTeam Home,
        FootballTeam Away,
        List<FootballPlayer> HomePlayers,
        List<FootballPlayer> AwayPlayers);

    private static ReadyMatch CreateReadyMatch(
        FootballMatchRules? rules = null,
        bool assignLineups = true,
        bool assignTeams = true)
    {
        FootballMatchRules matchRules = rules ?? FiveASideRules();
        FootballSeason season = CreateSeason(matchRules);
        (FootballTeam home, List<FootballPlayer> homePlayers) = CreateTeamWithSquad("Home", fieldPlayers: 6);
        (FootballTeam away, List<FootballPlayer> awayPlayers) = CreateTeamWithSquad("Away", fieldPlayers: 6);
        season.AddTeam(home);
        season.AddTeam(away);

        FootballMatch match = new(
            season,
            assignTeams ? home : null,
            assignTeams ? away : null,
            new DateTime(2027, 1, 15, 18, 30, 0, DateTimeKind.Utc),
            "Test Pitch");

        if (assignTeams && assignLineups)
        {
            SetStartingLineup(match, home, homePlayers, matchRules.PlayersOnField);
            SetStartingLineup(match, away, awayPlayers, matchRules.PlayersOnField);
        }

        return new ReadyMatch(match, home, away, homePlayers, awayPlayers);
    }

    [Fact]
    public void Constructor_AllowsBothTeamsNull()
    {
        FootballSeason season = CreateSeason();
        FootballMatch match = new(season, null, null, DateTime.UtcNow, "Pitch");

        match.HomeTeamId.Should().BeNull();
        match.AwayTeamId.Should().BeNull();
        match.Status.Should().Be(FootballMatchStatus.Scheduled);
        match.PeriodScores.Should().HaveCount(2);
        match.PeriodScores.Should().OnlyContain(ps => ps.HomeTeamId == Guid.Empty && ps.AwayTeamId == Guid.Empty);
    }

    [Fact]
    public void Start_Throws_WhenTeamIsMissing()
    {
        ReadyMatch ready = CreateReadyMatch(assignTeams: false, assignLineups: false);

        Action act = () => ready.Match.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*both teams*");
    }

    [Fact]
    public void Start_Throws_WhenLineupIsIncomplete()
    {
        ReadyMatch ready = CreateReadyMatch(assignLineups: false);

        Action act = () => ready.Match.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*on the field*");
    }

    [Fact]
    public void Start_DoesNotRequireOfficials_ByDefault()
    {
        ReadyMatch ready = CreateReadyMatch();

        ready.Match.Start();

        ready.Match.Status.Should().Be(FootballMatchStatus.InProgress);
        ready.Match.Officials.Should().BeEmpty();
    }

    [Fact]
    public void Start_RequiresOfficials_WhenRulesSaySo()
    {
        ReadyMatch ready = CreateReadyMatch(FiveASideRules(requireOfficials: true));

        Action act = () => ready.Match.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*officials*");
    }

    [Fact]
    public void RecordGoal_IncrementsScore()
    {
        ReadyMatch ready = CreateReadyMatch();
        ready.Match.Start();
        FootballPlayer scorer = ready.HomePlayers[1];

        FootballGoal goal = ready.Match.RecordGoal(ready.Home, scorer, assistingPlayer: null, periodNumber: 1, timeInSeconds: 125);

        ready.Match.HomeScore.Should().Be(1);
        ready.Match.AwayScore.Should().Be(0);
        goal.ScoringPlayerId.Should().Be(scorer.Id);
        ready.Match.PeriodScores.Single(ps => ps.PeriodNumber == 1).HomeScore.Should().Be(1);
    }

    [Fact]
    public void RecordGoal_OwnGoal_CreditsOpposingTeamButValidatesScorerOnOwnRoster()
    {
        ReadyMatch ready = CreateReadyMatch();
        ready.Match.Start();
        FootballPlayer scorer = ready.AwayPlayers[1];

        FootballGoal goal = ready.Match.RecordGoal(
            ready.Home,
            scorer,
            assistingPlayer: null,
            periodNumber: 1,
            timeInSeconds: 40,
            FootballGoalType.OwnGoal);

        ready.Match.HomeScore.Should().Be(1);
        goal.IsOwnGoal.Should().BeTrue();
        goal.TeamId.Should().Be(ready.Home.Id);
        goal.ScoringPlayerId.Should().Be(scorer.Id);
    }

    [Fact]
    public void RecordCard_SecondYellow_SendsPlayerOff()
    {
        ReadyMatch ready = CreateReadyMatch();
        ready.Match.Start();
        FootballPlayer player = ready.HomePlayers[1];

        ready.Match.RecordCard(ready.Home, player, FootballCardType.Yellow, 1, 10);
        FootballCard second = ready.Match.RecordCard(ready.Home, player, FootballCardType.Yellow, 1, 20);

        second.CardType.Should().Be(FootballCardType.SecondYellow);
        ready.Match.Lineup.Single(p => p.PlayerId == player.Id).IsSentOff.Should().BeTrue();
        ready.Match.Lineup.Single(p => p.PlayerId == player.Id).IsOnField.Should().BeFalse();
    }

    [Fact]
    public void RecordSubstitution_Unlimited_AllowsMultiple()
    {
        ReadyMatch ready = CreateReadyMatch();
        ready.Match.Start();
        FootballPlayer off = ready.HomePlayers[1];
        FootballPlayer on = ready.HomePlayers[5];

        ready.Match.RecordSubstitution(ready.Home, off, on, 1, 300);

        ready.Match.Lineup.Single(p => p.PlayerId == off.Id).IsOnField.Should().BeFalse();
        ready.Match.Lineup.Single(p => p.PlayerId == on.Id).IsOnField.Should().BeTrue();
        ready.Match.SubstitutionEvents.Should().HaveCount(1);
    }

    [Fact]
    public void RecordSubstitution_Throws_WhenLimitReached()
    {
        ReadyMatch ready = CreateReadyMatch(FiveASideRules(maxSubstitutions: 1));
        ready.Match.Start();
        FootballPlayer firstOff = ready.HomePlayers[1];
        FootballPlayer firstOn = ready.HomePlayers[5];
        ready.Match.RecordSubstitution(ready.Home, firstOff, firstOn, 1, 100);

        Action act = () => ready.Match.RecordSubstitution(ready.Home, firstOn, firstOff, 1, 200);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*substitutions*");
    }

    [Fact]
    public void RecordSubstitution_Throws_ForSentOffPlayer()
    {
        ReadyMatch ready = CreateReadyMatch();
        ready.Match.Start();
        FootballPlayer field = ready.HomePlayers[1];
        FootballPlayer bench = ready.HomePlayers[5];
        ready.Match.RecordCard(ready.Home, field, FootballCardType.DirectRed, 1, 10);

        Action act = () => ready.Match.RecordSubstitution(ready.Home, field, bench, 1, 20);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*sent-off*");
    }

    [Fact]
    public void Complete_AllowsDraw_InLeague()
    {
        ReadyMatch ready = CreateReadyMatch();
        ready.Match.Start();

        ready.Match.Complete();

        ready.Match.Status.Should().Be(FootballMatchStatus.Completed);
        ready.Match.HomeScore.Should().Be(0);
        ready.Match.AwayScore.Should().Be(0);
    }

    [Fact]
    public void Complete_Throws_WhenPlayoffIsDrawn()
    {
        ReadyMatch ready = CreateReadyMatch(FiveASideRules(allowExtraTime: true, allowPenaltyShootout: true));
        ready.Match.SetPlayoffInfo(FootballPlayoffRound.Final, 0, nextMatchId: null, nextMatchSlot: null);
        ready.Match.Start();

        Action act = () => ready.Match.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*draw*");
    }

    [Fact]
    public void AssignTeam_FillsSlotAndPeriodScores()
    {
        FootballSeason season = CreateSeason();
        (FootballTeam home, _) = CreateTeamWithSquad("Home", 4);
        (FootballTeam away, _) = CreateTeamWithSquad("Away", 4);
        season.AddTeam(home);
        season.AddTeam(away);
        FootballMatch match = new(season, null, null, DateTime.UtcNow, "Pitch");

        match.AssignTeam(FootballPlayoffSlot.Home, home);
        match.AssignTeam(FootballPlayoffSlot.Away, away);

        match.HomeTeamId.Should().Be(home.Id);
        match.AwayTeamId.Should().Be(away.Id);
        match.PeriodScores.Should().OnlyContain(ps => ps.HomeTeamId == home.Id && ps.AwayTeamId == away.Id);
    }

    [Fact]
    public void RecordExtraTime_AddsPeriodRows()
    {
        ReadyMatch ready = CreateReadyMatch(FiveASideRules(allowExtraTime: true, allowPenaltyShootout: true));
        ready.Match.Start();

        ready.Match.RecordExtraTime();
        ready.Match.RecordPenaltyShootout();

        ready.Match.WentToExtraTime.Should().BeTrue();
        ready.Match.WentToPenaltyShootout.Should().BeTrue();
        ready.Match.PeriodScores.Select(ps => ps.PeriodNumber).Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void MatchRules_RejectInvalidPlayerCount()
    {
        Action act = () => new FootballMatchRules(2, 20, 4, true, 0, false, false, 2, 15, false);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("playersOnField");
    }

    [Fact]
    public void StandingRules_AwardConfiguredPoints()
    {
        FootballStandingRules rules = new(3, 1, 0);

        rules.PointsFor(FootballGameResult.Win).Should().Be(3);
        rules.PointsFor(FootballGameResult.Draw).Should().Be(1);
        rules.PointsFor(FootballGameResult.Loss).Should().Be(0);
    }
}
