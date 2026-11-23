using Application.Common;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Handlers;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Handlers;
using Application.Features.Football.Statistics.Queries;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Football;

/// <summary>
/// Match statistics counted from events, clean sheets, missed penalties, and deleting a match with its totals.
/// </summary>
public class FootballMatchStatisticsHandlerTests
{
    private readonly Mock<IFootballMatchRepository> _matches = new();
    private readonly Mock<IFootballTeamRepository> _teams = new();
    private readonly Mock<IFootballPlayerRepository> _players = new();
    private readonly Mock<IFootballStatisticsRepository> _statistics = new();
    private readonly Mock<IMatchTimerService> _timer = new();
    private readonly IFootballUnitOfWork _unitOfWork = Mock.Of<IFootballUnitOfWork>();
    private readonly INotificationSenderService _notifications = Mock.Of<INotificationSenderService>();

    private readonly Dictionary<Guid, FootballPlayerSeasonStatistics> _playerRows = new();
    private readonly Dictionary<Guid, FootballTeamSeasonStatistics> _teamRows = new();

    private readonly FootballSeason _season;
    private readonly FootballTeam _home;
    private readonly FootballTeam _away;
    private readonly FootballPlayer _homeKeeper;
    private readonly FootballPlayer _homeBenchKeeper;
    private readonly FootballPlayer _homeScorer;
    private readonly FootballPlayer _awayKeeper;
    private readonly FootballPlayer _awayScorer;
    private readonly FootballMatch _match;

    public FootballMatchStatisticsHandlerTests()
    {
        _season = new FootballSeason(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        _home = new FootballTeam("Home", null, new Club("Home Club"), "Arena", "Blue", TeamCategory.Adult);
        _away = new FootballTeam("Away", null, new Club("Away Club"), "Arena", "Red", TeamCategory.Adult);
        _homeKeeper = AddPlayer(_home, FootballPosition.Goalkeeper, 1);
        _homeBenchKeeper = AddPlayer(_home, FootballPosition.Goalkeeper, 12);
        _homeScorer = AddPlayer(_home, FootballPosition.Forward, 9);
        _awayKeeper = AddPlayer(_away, FootballPosition.Goalkeeper, 1);
        _awayScorer = AddPlayer(_away, FootballPosition.Forward, 9);
        _season.AddTeam(_home);
        _season.AddTeam(_away);

        _match = new FootballMatch(
            _season, _home, _away, new DateTime(2027, 1, 15, 18, 0, 0, DateTimeKind.Utc), "Arena");
        _match.SetLineup(_home.Id,
        [
            new FootballLineupSelection(_homeKeeper.Id, FootballPosition.Goalkeeper, true),
            new FootballLineupSelection(_homeBenchKeeper.Id, FootballPosition.Goalkeeper, false),
            new FootballLineupSelection(_homeScorer.Id, FootballPosition.Forward, true),
        ]);
        _match.SetLineup(_away.Id,
        [
            new FootballLineupSelection(_awayKeeper.Id, FootballPosition.Goalkeeper, true),
            new FootballLineupSelection(_awayScorer.Id, FootballPosition.Forward, true),
        ]);
        _match.Start();

        _matches.Setup(r => r.GetByIdAsync(_match.Id)).ReturnsAsync(_match);
        _teams.Setup(r => r.GetByIdAsync(_home.Id)).ReturnsAsync(_home);
        _teams.Setup(r => r.GetByIdAsync(_away.Id)).ReturnsAsync(_away);
        SetupStatisticsRows();
    }

    [Fact]
    public async Task GetMatchStatistics_CountsGoalsAndCardsFromEvents()
    {
        await RecordGoalAsync(_home, _homeScorer);
        await RecordCardAsync(_away, _awayScorer, FootballCardType.Yellow);
        await RecordCardAsync(_away, _awayKeeper, FootballCardType.DirectRed);

        Result<List<FootballMatchTeamStatisticsDto>> result = await new GetMatchStatisticsHandler(_matches.Object)
            .Handle(new GetFootballMatchStatisticsQuery(_match.Id), CancellationToken.None);

        FootballMatchTeamStatisticsDto home = result.Data!.Single(s => s.TeamId == _home.Id);
        FootballMatchTeamStatisticsDto away = result.Data!.Single(s => s.TeamId == _away.Id);
        (home.Goals, home.YellowCards, home.RedCards, home.CleanSheet).Should().Be((1, 0, 0, true));
        (away.Goals, away.YellowCards, away.RedCards, away.CleanSheet).Should().Be((0, 1, 1, false));
    }

    [Fact]
    public async Task GetTeamPlayerStatistics_CountsCleanSheetsForGoalkeeperWhoPlayed()
    {
        _matches.Setup(r => r.GetCompletedWithLineupsAsync(_home.Id, _season.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([_match]);
        _statistics.Setup(r => r.GetPlayerStatisticsByTeamAndCompetitionAsync(_home.Id, _season.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateRow(_homeKeeper), CreateRow(_homeBenchKeeper), CreateRow(_homeScorer)]);
        Mock<IPersonRepository> persons = new();
        persons.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new List<Person>());

        Result<List<FootballPlayerSeasonStatisticsDto>> result = await new GetTeamPlayerStatisticsHandler(
            _matches.Object, _statistics.Object, persons.Object, Mock.Of<ILogger<GetTeamPlayerStatisticsHandler>>())
            .Handle(new GetFootballTeamPlayerStatisticsQuery(_season.Id, _home.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Data!.Single(s => s.PlayerId == _homeKeeper.Id).CleanSheets.Should().Be(1);
        result.Data!.Single(s => s.PlayerId == _homeBenchKeeper.Id).CleanSheets.Should().Be(0);
        result.Data!.Single(s => s.PlayerId == _homeScorer.Id).CleanSheets.Should().Be(0);
    }

    [Fact]
    public async Task DeleteMatch_Completed_RevertsEveryTotal()
    {
        await RecordGoalAsync(_home, _homeScorer);
        await RecordCardAsync(_away, _awayScorer, FootballCardType.Yellow);
        Result<FootballMatchDto> completed = await new CompleteFootballMatchHandler(
            _matches.Object, _statistics.Object, Mock.Of<IFootballTournamentRepository>(), _teams.Object, _unitOfWork,
            _timer.Object, _notifications, Mock.Of<ILogger<CompleteFootballMatchHandler>>())
            .Handle(new CompleteFootballMatchCommand(_match.Id), CancellationToken.None);
        completed.IsSuccess.Should().BeTrue(completed.Error);

        Result result = await DeleteMatchAsync();

        result.IsSuccess.Should().BeTrue(result.Error);
        _matches.Verify(r => r.DeleteAsync(_match.Id), Times.Once);
        _teamRows.Values.Should().OnlyContain(t =>
            t.GoalsFor == 0 && t.GoalsAgainst == 0 && t.Points == 0 && t.GamesPlayed == 0 && t.CleanSheets == 0);
        _playerRows.Values.Should().OnlyContain(p => p.Goals == 0 && p.YellowCards == 0 && p.GamesPlayed == 0);
    }

    [Fact]
    public async Task DeleteMatch_InProgress_DestroysTimer()
    {
        Result result = await DeleteMatchAsync();

        result.IsSuccess.Should().BeTrue(result.Error);
        _timer.Verify(t => t.DestroyTimerAsync(_match.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteMatch_StartedPlayoffMatch_ReturnsFailure()
    {
        _match.SetPlayoffInfo(FootballPlayoffRound.Final, 0, null, null);

        Result result = await DeleteMatchAsync();

        result.IsFailure.Should().BeTrue();
        _matches.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RecordMissedPenalty_AddsEventToDto()
    {
        Result<FootballMatchDto> result = await new RecordFootballMissedPenaltyHandler(
            _matches.Object, _teams.Object, _players.Object, _unitOfWork, _notifications)
            .Handle(
                new RecordFootballMissedPenaltyCommand(_match.Id, _home.Id, _homeScorer.Id, 1, 120),
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Data!.MissedPenaltyEvents.Should().ContainSingle(e => e.PlayerId == _homeScorer.Id);
        result.Data.HomeScore.Should().Be(0);
    }

    [Fact]
    public async Task SetDecision_NotAllowedByRules_ReturnsFailure()
    {
        Result<FootballMatchDto> result = await new SetFootballMatchDecisionHandler(
            _matches.Object, _unitOfWork, _notifications)
            .Handle(new SetFootballMatchDecisionCommand(_match.Id, MatchDecision.Shootout), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not allowed");
    }

    private FootballPlayer AddPlayer(FootballTeam team, FootballPosition position, int jerseyNumber)
    {
        FootballPlayer player = new(Guid.NewGuid(), new FootballPositionPreference(position));
        team.AddPlayer(player, position, jerseyNumber);
        _players.Setup(r => r.GetByIdAsync(player.Id)).ReturnsAsync(player);
        return player;
    }

    private FootballPlayerSeasonStatistics CreateRow(FootballPlayer player)
    {
        FootballPlayerSeasonStatistics row = new(player.Id, _home.Id, _season.Id);
        typeof(FootballPlayerSeasonStatistics).GetProperty("Player")!.SetValue(row, player);
        typeof(FootballPlayerSeasonStatistics).GetProperty("Team")!.SetValue(row, _home);
        typeof(FootballPlayerSeasonStatistics).GetProperty("Competition")!.SetValue(row, _season);
        return row;
    }

    private async Task RecordGoalAsync(FootballTeam team, FootballPlayer scorer)
    {
        Result<FootballMatchDto> result = await new RecordGoalHandler(
            _matches.Object, _teams.Object, _players.Object, _statistics.Object, _unitOfWork, _notifications,
            Mock.Of<ILogger<RecordGoalHandler>>())
            .Handle(
                new RecordFootballGoalCommand(_match.Id, team.Id, scorer.Id, null, 1, 60, null, null),
                CancellationToken.None);
        result.IsSuccess.Should().BeTrue(result.Error);
    }

    private async Task RecordCardAsync(FootballTeam team, FootballPlayer player, FootballCardType cardType)
    {
        Result<FootballMatchDto> result = await new RecordCardHandler(
            _matches.Object, _teams.Object, _players.Object, _statistics.Object, _unitOfWork, _notifications,
            Mock.Of<ILogger<RecordCardHandler>>())
            .Handle(
                new RecordFootballCardCommand(_match.Id, team.Id, player.Id, cardType, 1, 90, null),
                CancellationToken.None);
        result.IsSuccess.Should().BeTrue(result.Error);
    }

    private Task<Result> DeleteMatchAsync() =>
        new DeleteFootballMatchHandler(
            _matches.Object, _players.Object, _statistics.Object, _timer.Object, _unitOfWork,
            Mock.Of<ILogger<DeleteFootballMatchHandler>>())
            .Handle(new DeleteFootballMatchCommand(_match.Id), CancellationToken.None);

    private void SetupStatisticsRows()
    {
        _statistics.Setup(r => r.GetPlayerSeasonStatisticsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid playerId, Guid _, Guid _, CancellationToken _) => _playerRows.GetValueOrDefault(playerId));
        _statistics.Setup(r => r.SavePlayerSeasonStatisticsAsync(
                It.IsAny<FootballPlayerSeasonStatistics>(), It.IsAny<CancellationToken>()))
            .Callback((FootballPlayerSeasonStatistics row, CancellationToken _) => _playerRows[row.PlayerId] = row)
            .Returns(Task.CompletedTask);
        _statistics.Setup(r => r.GetTeamSeasonStatisticsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid teamId, Guid _, CancellationToken _) => _teamRows.GetValueOrDefault(teamId));
        _statistics.Setup(r => r.SaveTeamSeasonStatisticsAsync(
                It.IsAny<FootballTeamSeasonStatistics>(), It.IsAny<CancellationToken>()))
            .Callback((FootballTeamSeasonStatistics row, CancellationToken _) => _teamRows[row.TeamId] = row)
            .Returns(Task.CompletedTask);
    }
}
