using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Statistics.Queries;
using Application.Features.Football.Statistics.Queries;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Hockey.Statistics.DTOs;
using Application.Features.Hockey.Statistics.Handlers;
using Application.Features.Hockey.Statistics.Queries;
using Application.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Teams;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Enums.Football;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using Domain.ValueObjects.Floorball;
using Domain.ValueObjects.Football;
using Domain.ValueObjects.Hockey.Rules;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using FloorballStandingsHandler = Application.Features.Floorball.Statistics.Handlers.GetTeamStandingsHandler;
using FootballStandingsHandler = Application.Features.Football.Statistics.Handlers.GetTeamStandingsHandler;

namespace ApplicationTestProject.Handlers.Statistics;

public class SeasonStandingPointTableTests
{
    private static readonly DateTime Start = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task FloorballTable_UsesSeasonRulesForRegulationAndOvertime()
    {
        FloorballSeason season = new("Salibandy", Start, End);
        season.UpdateStandingRules(new FloorballStandingRules(5, 2, 4, 1));
        (FloorballTeam home, FloorballPlayer homeScorer, FloorballPlayer homeGoalie) = CreateFloorballSide("Home");
        (FloorballTeam away, FloorballPlayer awayScorer, FloorballPlayer awayGoalie) = CreateFloorballSide("Away");
        season.AddTeam(home);
        season.AddTeam(away);
        season.Activate();

        FloorballMatch regulation = CreateCompletedFloorballMatch(season, home, away, homeScorer, homeGoalie, awayGoalie, overtime: false);
        FloorballMatch overtime = CreateCompletedFloorballMatch(season, away, home, awayScorer, awayGoalie, homeGoalie, overtime: true);

        FloorballStandingsHandler handler = CreateFloorballHandler(season, [regulation, overtime]);
        Result<List<FloorballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFloorballTeamStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<FloorballTeamSeasonStatisticsDto> floorballRows = result.Data ?? [];
        FloorballTeamSeasonStatisticsDto homeRow = floorballRows.Single(row => row.TeamId == home.Id);
        FloorballTeamSeasonStatisticsDto awayRow = floorballRows.Single(row => row.TeamId == away.Id);
        homeRow.Points.Should().Be(5 + 1);
        awayRow.Points.Should().Be(0 + 4);
    }

    [Fact]
    public async Task FootballTable_UsesOvertimePointsWhenTheMatchLeavesRegulation()
    {
        FootballMatchRules matchRules = new(2, 20, 5, true, 0, false, allowExtraTime: true, 2, 5, false);
        FootballStandingRules standingRules = new(5, 2, 0, overtimeWinPoints: 4, overtimeLossPoints: 1);
        FootballSeason season = new("Jalkapallo", Start, End, matchRules, standingRules);
        (FootballTeam home, List<FootballPlayer> homePlayers) = CreateFootballSide("Home");
        (FootballTeam away, List<FootballPlayer> awayPlayers) = CreateFootballSide("Away");
        season.AddTeam(home);
        season.AddTeam(away);
        season.Activate();

        FootballMatch regulation = CreateCompletedFootballMatch(season, home, away, homePlayers, awayPlayers, extraTime: false);
        FootballMatch extraTime = CreateCompletedFootballMatch(season, away, home, awayPlayers, homePlayers, extraTime: true);

        FootballStandingsHandler handler = CreateFootballHandler(season, [regulation, extraTime]);
        Result<List<FootballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFootballTeamStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<FootballTeamSeasonStatisticsDto> footballRows = result.Data ?? [];
        footballRows.Single(row => row.TeamId == home.Id).Points.Should().Be(5 + 1);
        footballRows.Single(row => row.TeamId == away.Id).Points.Should().Be(0 + 4);
    }

    [Fact]
    public async Task HockeyTable_ReplacesStoredPointsWithRegulationOvertimeShootoutAndTiePoints()
    {
        HockeySeason season = new("Jääkiekko", Start, End);
        Guid homeId = Guid.NewGuid();
        Guid awayId = Guid.NewGuid();
        HockeyCompetitionTeam homeMembership = season.AddTeam(homeId);
        HockeyCompetitionTeam awayMembership = season.AddTeam(awayId);
        season.Publish();
        season.Activate();
        HockeyStandingRules custom = new(4, 3, 2, 1, 0, 1, season.GetEffectiveRules().StandingRules.TieBreakers);
        season.UpdateCompetitionRules(season.GetEffectiveRules().WithStandingRules(custom));

        HockeyMatch regulation = CreateHockeyResult(season.Id, homeMembership, awayMembership, 3, 1, HockeyMatchResultType.HomeWin);
        HockeyMatch overtime = CreateHockeyResult(season.Id, homeMembership, awayMembership, 2, 3, HockeyMatchResultType.OvertimeAwayWin);
        HockeyMatch shootout = CreateHockeyResult(season.Id, homeMembership, awayMembership, 4, 3, HockeyMatchResultType.ShootoutHomeWin);
        HockeyMatch draw = CreateHockeyResult(season.Id, homeMembership, awayMembership, 1, 1, HockeyMatchResultType.Draw);

        GetHockeyCompetitionStandingsHandler handler = CreateHockeyHandler(
            season,
            new Dictionary<Guid, string> { [homeId] = "Home", [awayId] = "Away" },
            [regulation, overtime, shootout, draw]);

        Result<List<HockeyTeamCompetitionStatisticsDto>> result = await handler.Handle(
            new GetHockeyCompetitionStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<HockeyTeamCompetitionStatisticsDto> hockeyRows = result.Data ?? [];
        hockeyRows.Single(row => row.TeamId == homeId).Points.Should().Be(4 + 1 + 2 + 1);
        hockeyRows.Single(row => row.TeamId == awayId).Points.Should().Be(0 + 3 + 0 + 1);
    }

    private static (FloorballTeam Team, FloorballPlayer Scorer, FloorballPlayer Goalie) CreateFloorballSide(string name)
    {
        Club club = new(name + " HC");
        FloorballTeam team = new(name, null, club, "Arena", "Blue", TeamCategory.Adult);
        FloorballPlayer goalie = new(new Person("Goal", name).Id, new Position(FloorballPosition.Goalkeeper, canPlayAsGoalkeeper: true));
        FloorballPlayer scorer = new(new Person("Score", name).Id, new Position(FloorballPosition.Forward));
        team.AddPlayer(goalie, FloorballPosition.Goalkeeper, jerseyNumber: 1);
        team.AddPlayer(scorer, FloorballPosition.Forward, jerseyNumber: 10);
        return (team, scorer, goalie);
    }

    private static FloorballMatch CreateCompletedFloorballMatch(
        FloorballSeason season,
        FloorballTeam winner,
        FloorballTeam loser,
        FloorballPlayer scorer,
        FloorballPlayer winnerGoalie,
        FloorballPlayer loserGoalie,
        bool overtime)
    {
        FloorballMatch match = new(
            season,
            winner,
            loser,
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
            "Arena");
        match.AddOfficial(new FloorballReferee(Guid.NewGuid(), Start.AddYears(-1), End.AddYears(1)));
        match.SetActiveGoalie(winner.Id, winnerGoalie.Id);
        match.SetActiveGoalie(loser.Id, loserGoalie.Id);
        match.Start();
        if (overtime)
            match.RecordOvertime();

        int period = overtime ? match.OvertimePeriodNumber : 1;
        match.RecordGoal(winner, scorer, null, null, period, 30);
        match.Complete();
        return match;
    }

    private static (FootballTeam Team, List<FootballPlayer> Players) CreateFootballSide(string name)
    {
        Club club = new(name + " FC");
        FootballTeam team = new(name, null, club, "Pitch", "Red", TeamCategory.Adult);
        List<FootballPlayer> players = new();
        FootballPlayer goalkeeper = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Goalkeeper));
        team.AddPlayer(goalkeeper, FootballPosition.Goalkeeper, jerseyNumber: 1);
        players.Add(goalkeeper);
        for (int index = 0; index < 4; index++)
        {
            FootballPlayer player = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));
            team.AddPlayer(player, FootballPosition.Midfielder, jerseyNumber: index + 2);
            players.Add(player);
        }

        return (team, players);
    }

    private static FootballMatch CreateCompletedFootballMatch(
        FootballSeason season,
        FootballTeam winner,
        FootballTeam loser,
        IReadOnlyList<FootballPlayer> winnerPlayers,
        IReadOnlyList<FootballPlayer> loserPlayers,
        bool extraTime)
    {
        FootballMatch match = new(
            season,
            winner,
            loser,
            new DateTime(2026, 10, 2, 18, 0, 0, DateTimeKind.Utc),
            "Pitch");
        SetLineup(match, winner, winnerPlayers);
        SetLineup(match, loser, loserPlayers);
        match.Start();
        if (extraTime)
            match.RecordExtraTime();

        int period = extraTime ? match.MatchRules.ExtraTimeStartPeriodNumber : 1;
        match.RecordGoal(winner, winnerPlayers[1], null, period, 40);
        match.Complete();
        return match;
    }

    private static void SetLineup(FootballMatch match, FootballTeam team, IReadOnlyList<FootballPlayer> players)
    {
        List<FootballLineupSelection> selections = new();
        for (int index = 0; index < players.Count; index++)
        {
            FootballPosition position = index == 0 ? FootballPosition.Goalkeeper : FootballPosition.Midfielder;
            selections.Add(new FootballLineupSelection(players[index].Id, position, IsOnField: true));
        }

        match.SetLineup(team.Id, selections);
    }

    private static HockeyMatch CreateHockeyResult(
        Guid competitionId,
        HockeyCompetitionTeam home,
        HockeyCompetitionTeam away,
        int homeGoals,
        int awayGoals,
        HockeyMatchResultType resultType)
    {
        HockeyMatch match = new(
            new DateTime(2026, 10, 3, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.League,
            competitionId: competitionId);
        match.AssignMatchTeam(home.TeamId, HockeyTeamSlot.Home, home);
        match.AssignMatchTeam(away.TeamId, HockeyTeamSlot.Away, away);
        match.SetTeamGoals(HockeyTeamSlot.Home, homeGoals);
        match.SetTeamGoals(HockeyTeamSlot.Away, awayGoals);
        match.SetResultType(resultType);
        match.MarkFinished(resultType: resultType);
        return match;
    }

    private static FloorballStandingsHandler CreateFloorballHandler(
        FloorballSeason season,
        IReadOnlyList<FloorballMatch> matches)
    {
        Mock<IFloorballStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamStandingsAsync(season.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Floorball.Statistics.FloorballTeamSeasonStatistics>());
        Mock<IFloorballCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(season.Id)).ReturnsAsync(season);
        Mock<IFloorballTournamentRepository> tournaments = new();
        Mock<IFloorballMatchRepository> matchRepository = new();
        matchRepository.Setup(repository => repository.GetByCompetitionIdAsync(season.Id)).ReturnsAsync(matches.ToList());
        Mock<IClubRepository> clubs = new();
        clubs
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Club>());

        return new FloorballStandingsHandler(
            statistics.Object,
            competitions.Object,
            tournaments.Object,
            new Mock<IFloorballTeamRepository>().Object,
            clubs.Object,
            matchRepository.Object,
            Mock.Of<ILogger<FloorballStandingsHandler>>());
    }

    private static FootballStandingsHandler CreateFootballHandler(
        FootballSeason season,
        IReadOnlyList<FootballMatch> matches)
    {
        Mock<IFootballStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamStandingsAsync(season.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Football.Statistics.FootballTeamSeasonStatistics>());
        Mock<IFootballCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(season.Id)).ReturnsAsync(season);
        Mock<IFootballTournamentRepository> tournaments = new();
        Mock<IFootballMatchRepository> matchRepository = new();
        matchRepository.Setup(repository => repository.GetByCompetitionIdAsync(season.Id)).ReturnsAsync(matches.ToList());
        Mock<IClubRepository> clubs = new();
        clubs
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Club>());

        return new FootballStandingsHandler(
            statistics.Object,
            competitions.Object,
            tournaments.Object,
            new Mock<IFootballTeamRepository>().Object,
            clubs.Object,
            matchRepository.Object,
            Mock.Of<ILogger<FootballStandingsHandler>>());
    }

    private static GetHockeyCompetitionStandingsHandler CreateHockeyHandler(
        HockeySeason season,
        IReadOnlyDictionary<Guid, string> names,
        IReadOnlyList<HockeyMatch> matches)
    {
        Mock<IHockeyStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamCompetitionStatisticsAsync(
                season.Id,
                HockeyStatisticsScope.Competition,
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(new List<Domain.Entities.Hockey.Statistics.HockeyTeamCompetitionStatistics>());
        Mock<IHockeyCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(season.Id)).ReturnsAsync(season);
        Mock<IHockeyTeamRepository> teams = new();
        teams
            .Setup(repository => repository.GetNamesByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(names);
        Mock<IHockeyMatchRepository> matchRepository = new();
        matchRepository.Setup(repository => repository.GetByCompetitionIdAsync(season.Id)).ReturnsAsync(matches.ToList());

        return new GetHockeyCompetitionStandingsHandler(
            statistics.Object,
            competitions.Object,
            teams.Object,
            matchRepository.Object,
            Mock.Of<ILogger<GetHockeyCompetitionStandingsHandler>>());
    }
}
