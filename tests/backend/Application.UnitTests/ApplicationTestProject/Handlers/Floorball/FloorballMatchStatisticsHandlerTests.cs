using Application.Common;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Handlers;
using Application.Features.Floorball.Statistics.Handlers;
using Application.Features.Floorball.Statistics.Queries;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

/// <summary>
/// Runs the floorball match handlers against one in-memory set of statistics rows, so recording,
/// correcting, reopening and deleting can be checked to leave the totals consistent.
/// </summary>
public class FloorballMatchStatisticsHandlerTests
{
    private readonly Mock<IFloorballMatchRepository> _matches = new();
    private readonly Mock<IFloorballTeamRepository> _teams = new();
    private readonly Mock<IFloorballPlayerRepository> _players = new();
    private readonly Mock<IFloorballStatisticsRepository> _statistics = new();
    private readonly Mock<IMatchTimerService> _timer = new();
    private readonly IFloorballUnitOfWork _unitOfWork = Mock.Of<IFloorballUnitOfWork>();
    private readonly INotificationSenderService _notifications = Mock.Of<INotificationSenderService>();

    private readonly Dictionary<Guid, FloorballGoalieSeasonStatistics> _goalieRows = new();
    private readonly Dictionary<Guid, FloorballPlayerSeasonStatistics> _playerRows = new();
    private readonly Dictionary<Guid, FloorballTeamSeasonStatistics> _teamRows = new();
    private readonly Dictionary<Guid, FloorballMatchTeamStatistics> _matchTeamRows = new();

    private readonly FloorballSeason _season;
    private readonly FloorballTeam _home;
    private readonly FloorballTeam _away;
    private readonly FloorballPlayer _homeGoalie;
    private readonly FloorballPlayer _awayGoalie;
    private readonly FloorballPlayer _awaySecondGoalie;
    private readonly FloorballPlayer _homeScorer;
    private readonly FloorballPlayer _awayScorer;
    private readonly FloorballMatch _match;

    public FloorballMatchStatisticsHandlerTests()
    {
        _season = new FloorballSeason(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        _home = new FloorballTeam("Home", null, new Club("Home Club"), "Arena", "Blue", TeamCategory.Adult);
        _away = new FloorballTeam("Away", null, new Club("Away Club"), "Arena", "Red", TeamCategory.Adult);
        _homeGoalie = AddPlayer(_home, FloorballPosition.Goalkeeper, 1);
        _homeScorer = AddPlayer(_home, FloorballPosition.Forward, 9);
        _awayGoalie = AddPlayer(_away, FloorballPosition.Goalkeeper, 1);
        _awaySecondGoalie = AddPlayer(_away, FloorballPosition.Goalkeeper, 30);
        _awayScorer = AddPlayer(_away, FloorballPosition.Forward, 9);
        _season.AddTeam(_home);
        _season.AddTeam(_away);

        _match = new FloorballMatch(
            _season, _home, _away, new DateTime(2027, 1, 15, 18, 0, 0, DateTimeKind.Utc), "Arena");
        _match.AddOfficial(new FloorballReferee(
            Guid.NewGuid(),
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        _match.SetActiveGoalie(_home.Id, _homeGoalie.Id);
        _match.SetActiveGoalie(_away.Id, _awayGoalie.Id);
        _match.Start();

        _matches.Setup(r => r.GetByIdAsync(_match.Id)).ReturnsAsync(_match);
        _teams.Setup(r => r.GetByIdAsync(_home.Id)).ReturnsAsync(_home);
        _teams.Setup(r => r.GetByIdAsync(_away.Id)).ReturnsAsync(_away);
        SetupStatisticsRows();
    }

    [Fact]
    public async Task Saves_DeletedAndRecordedAgainForAnotherGoalie_KeepTeamAndGoalieTotals()
    {
        await RecordGoalAsync(_home, _homeScorer);
        await RecordSavesAsync(_away, _awayGoalie, 37);
        await RecordSavesAsync(_home, _homeGoalie, 5);

        foreach (FloorballSave save in _match.SaveEvents.Where(s => s.TeamId == _away.Id).ToList())
        {
            Result<FloorballMatchDto> deleted = await new DeleteSaveHandler(
                _matches.Object, _players.Object, _statistics.Object, _unitOfWork, _notifications, Mock.Of<ILogger<DeleteSaveHandler>>())
                .Handle(new DeleteFloorballSaveCommand(_match.Id, save.Id), CancellationToken.None);
            deleted.IsSuccess.Should().BeTrue(deleted.Error);
        }

        await RecordSavesAsync(_away, _awaySecondGoalie, 37);

        _matchTeamRows[_home.Id].ShotsTotal.Should().Be(38);
        _matchTeamRows[_away.Id].ShotsTotal.Should().Be(5);
        _goalieRows[_awayGoalie.Id].Saves.Should().Be(0);
        _goalieRows[_awayGoalie.Id].GoalsAgainst.Should().Be(1);
        _goalieRows[_awaySecondGoalie.Id].Saves.Should().Be(37);
    }

    [Fact]
    public async Task GetMatchStatistics_StoredTotalsDrifted_ReturnsCountsFromEvents()
    {
        await RecordGoalAsync(_home, _homeScorer);
        await RecordSavesAsync(_away, _awayGoalie, 4);
        await RecordSavesAsync(_home, _homeGoalie, 2);
        _matchTeamRows[_home.Id].SetShotStatistics(1, 79);
        _matchTeamRows[_away.Id].SetShotStatistics(0, 0);
        _statistics.Setup(r => r.GetMatchStatisticsAsync(_match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_matchTeamRows.Values);

        Result<List<FloorballMatchTeamStatisticsDto>> result = await new GetMatchStatisticsHandler(
            _matches.Object, _statistics.Object, Mock.Of<ILogger<GetMatchStatisticsHandler>>())
            .Handle(new GetFloorballMatchStatisticsQuery(_match.Id), CancellationToken.None);

        result.Data!.Single(s => s.TeamId == _home.Id).ShotsTotal.Should().Be(5);
        result.Data!.Single(s => s.TeamId == _away.Id).ShotsTotal.Should().Be(2);
    }

    [Fact]
    public async Task SetActiveRoster_MoveGoalieEvents_MovesSeasonTotalsToNewGoalie()
    {
        await RecordGoalAsync(_home, _homeScorer);
        await RecordSavesAsync(_away, _awayGoalie, 6);

        Result<FloorballMatchDto> result = await new SetMatchActiveRosterHandler(
            _matches.Object, _teams.Object, _statistics.Object, _unitOfWork, Mock.Of<ILogger<SetMatchActiveRosterHandler>>())
            .Handle(
                new SetFloorballMatchActiveRosterCommand
                {
                    MatchId = _match.Id,
                    TeamId = _away.Id,
                    GoalieId = _awaySecondGoalie.Id,
                    MoveGoalieEvents = true,
                },
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        _goalieRows[_awayGoalie.Id].Saves.Should().Be(0);
        _goalieRows[_awayGoalie.Id].GoalsAgainst.Should().Be(0);
        _goalieRows[_awaySecondGoalie.Id].Saves.Should().Be(6);
        _goalieRows[_awaySecondGoalie.Id].GoalsAgainst.Should().Be(1);
        _goalieRows[_awaySecondGoalie.Id].ShotsAgainst.Should().Be(7);
    }

    [Fact]
    public async Task Complete_SharedMatch_GivesEachGoalieOwnGoalsAndAppearance()
    {
        await RecordGoalAsync(_home, _homeScorer);
        _match.SetActiveGoalie(_away.Id, _awaySecondGoalie.Id);
        await RecordGoalAsync(_home, _homeScorer);
        await RecordGoalAsync(_home, _homeScorer);

        await CompleteAsync();

        FloorballGoalieSeasonStatistics first = _goalieRows[_awayGoalie.Id];
        FloorballGoalieSeasonStatistics second = _goalieRows[_awaySecondGoalie.Id];
        (first.GamesPlayed, first.Losses, first.GoalsAgainstAverage).Should().Be((1, 0, 1m));
        (second.GamesPlayed, second.Losses, second.GoalsAgainstAverage).Should().Be((1, 1, 2m));
    }

    [Fact]
    public async Task DeleteGoal_AfterGoalieChange_UnchargesGoalieWhoConceded()
    {
        FloorballGoal goal = await RecordGoalAsync(_home, _homeScorer);
        _match.SetActiveGoalie(_away.Id, _awaySecondGoalie.Id);

        await DeleteGoalAsync(goal);

        _goalieRows[_awayGoalie.Id].GoalsAgainst.Should().Be(0);
        _matchTeamRows[_home.Id].ShotsTotal.Should().Be(0);
    }

    [Fact]
    public async Task DeleteGoal_OwnGoal_KeepsScorersOtherGoals()
    {
        await RecordGoalAsync(_home, _homeScorer);
        FloorballGoal ownGoal = await RecordGoalAsync(_home, _homeScorer, FloorballGoalType.OwnGoal);

        await DeleteGoalAsync(ownGoal);

        _playerRows[_homeScorer.Id].Goals.Should().Be(1);
        _goalieRows[_awayGoalie.Id].GoalsAgainst.Should().Be(1);
        _teamRows[_home.Id].GoalsFor.Should().Be(1);
    }

    [Fact]
    public async Task SetDecision_Shootout_ReopenedMatch_GivesShootoutPoints()
    {
        await RecordGoalAsync(_away, _awayScorer);
        await RecordGoalAsync(_home, _homeScorer);
        await RecordGoalAsync(_away, _awayScorer, FloorballGoalType.Shootout);
        await CompleteAsync();
        int regulationPoints = _teamRows[_away.Id].Points;
        await ReopenAsync();

        Result<FloorballMatchDto> result = await new SetFloorballMatchDecisionHandler(
            _matches.Object, _statistics.Object, _unitOfWork, _notifications)
            .Handle(new SetFloorballMatchDecisionCommand(_match.Id, MatchDecision.Shootout), CancellationToken.None);
        await CompleteAsync();

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Data!.WentToShootout.Should().BeTrue();
        _teamRows[_away.Id].Points.Should().BeLessThan(regulationPoints);
        _teamRows[_home.Id].Points.Should().Be(_season.StandingRules.OvertimeLossPoints);
    }

    [Fact]
    public async Task SetDecision_MissingMatch_ReturnsNotFound()
    {
        Result<FloorballMatchDto> result = await new SetFloorballMatchDecisionHandler(
            _matches.Object, _statistics.Object, _unitOfWork, _notifications)
            .Handle(new SetFloorballMatchDecisionCommand(Guid.NewGuid(), MatchDecision.Shootout), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorKind.Should().Be(ResultErrorKind.NotFound);
    }

    [Fact]
    public async Task DeleteMatch_Completed_RevertsEveryTotal()
    {
        await RecordGoalAsync(_home, _homeScorer);
        await RecordGoalAsync(_away, _awayScorer, FloorballGoalType.OwnGoal);
        await RecordSavesAsync(_away, _awayGoalie, 8);
        await new RecordPenaltyHandler(
            _matches.Object, _teams.Object, _players.Object, _statistics.Object, _notifications, _unitOfWork,
            Mock.Of<ILogger<RecordPenaltyHandler>>())
            .Handle(
                new RecordFloorballPenaltyCommand(
                    _match.Id, _home.Id, _homeScorer.Id, FloorballPenaltyType.Minor, 4, 1, 300, string.Empty),
                CancellationToken.None);
        await CompleteAsync();

        Result result = await DeleteMatchAsync();

        result.IsSuccess.Should().BeTrue(result.Error);
        _matches.Verify(r => r.DeleteAsync(_match.Id), Times.Once);
        _statistics.Verify(r => r.RemoveMatchTeamStatisticsAsync(_match.Id, It.IsAny<CancellationToken>()), Times.Once);
        _teamRows.Values.Should().OnlyContain(t =>
            t.GoalsFor == 0 && t.GoalsAgainst == 0 && t.Points == 0 && t.GamesPlayed == 0);
        _playerRows.Values.Should().OnlyContain(p => p.Goals == 0 && p.PenaltyMinutes == 0 && p.GamesPlayed == 0);
        _goalieRows.Values.Should().OnlyContain(g =>
            g.GamesPlayed == 0 && g.Saves == 0 && g.GoalsAgainst == 0 && g.ShotsAgainst == 0);
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
        _match.SetPlayoffInfo(FloorballPlayoffRound.Final, 0, null, null);

        Result result = await DeleteMatchAsync();

        result.IsFailure.Should().BeTrue();
        _matches.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RecordMissedPenalty_AddsEventToDtoWithoutTouchingStatistics()
    {
        Result<FloorballMatchDto> result = await new RecordFloorballMissedPenaltyHandler(
            _matches.Object, _teams.Object, _players.Object, _unitOfWork, _notifications)
            .Handle(
                new RecordFloorballMissedPenaltyCommand(_match.Id, _home.Id, _homeScorer.Id, 1, 120),
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Data!.MissedPenaltyEvents.Should().ContainSingle(e => e.PlayerId == _homeScorer.Id);
        _playerRows.Should().BeEmpty();
    }

    [Fact]
    public async Task RecordMissedPenalty_ShooterFromOtherTeam_ReturnsFailure()
    {
        Result<FloorballMatchDto> result = await new RecordFloorballMissedPenaltyHandler(
            _matches.Object, _teams.Object, _players.Object, _unitOfWork, _notifications)
            .Handle(
                new RecordFloorballMissedPenaltyCommand(_match.Id, _home.Id, _awayScorer.Id, 1, 120),
                CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    private FloorballPlayer AddPlayer(FloorballTeam team, FloorballPosition position, int jerseyNumber)
    {
        FloorballPlayer player = new(Guid.NewGuid(), new Position(position));
        team.AddPlayer(player, position, jerseyNumber);
        _players.Setup(r => r.GetByIdAsync(player.Id)).ReturnsAsync(player);
        return player;
    }

    private async Task<FloorballGoal> RecordGoalAsync(
        FloorballTeam team, FloorballPlayer scorer, FloorballGoalType? goalType = null)
    {
        Result<FloorballMatchDto> result = await new RecordGoalHandler(
            _matches.Object, _teams.Object, _players.Object, _statistics.Object, _unitOfWork, _notifications,
            Mock.Of<ILogger<RecordGoalHandler>>())
            .Handle(
                new RecordFloorballGoalCommand(
                    _match.Id, team.Id, scorer.Id, null, null, 1, 60 + _match.Events.Count, null, (int?)goalType),
                CancellationToken.None);
        result.IsSuccess.Should().BeTrue(result.Error);
        return _match.GoalEvents.Last();
    }

    private async Task RecordSavesAsync(FloorballTeam team, FloorballPlayer goalie, int count)
    {
        Result<FloorballMatchDto> result = await new RecordSaveHandler(
            _matches.Object, _teams.Object, _players.Object, _statistics.Object, _unitOfWork, _notifications,
            Mock.Of<ILogger<RecordSaveHandler>>())
            .Handle(
                new RecordFloorballSaveCommand(_match.Id, team.Id, goalie.Id, 1, 100, false, false, count),
                CancellationToken.None);
        result.IsSuccess.Should().BeTrue(result.Error);
    }

    private async Task DeleteGoalAsync(FloorballGoal goal)
    {
        Result<FloorballMatchDto> result = await new DeleteGoalHandler(
            _matches.Object, _teams.Object, _players.Object, _statistics.Object, _unitOfWork, Mock.Of<ILogger<DeleteGoalHandler>>())
            .Handle(new DeleteFloorballGoalCommand(_match.Id, goal.Id), CancellationToken.None);
        result.IsSuccess.Should().BeTrue(result.Error);
    }

    private async Task CompleteAsync()
    {
        Result<FloorballMatchDto> result = await new CompleteFloorballMatchHandler(
            _matches.Object, _statistics.Object, Mock.Of<IFloorballTournamentRepository>(), _teams.Object, _unitOfWork,
            _timer.Object, _notifications, Mock.Of<ILogger<CompleteFloorballMatchHandler>>())
            .Handle(new CompleteFloorballMatchCommand(_match.Id), CancellationToken.None);
        result.IsSuccess.Should().BeTrue(result.Error);
    }

    private async Task ReopenAsync()
    {
        Result<FloorballMatchDto> result = await new ReopenFloorballMatchHandler(
            _matches.Object, _statistics.Object, _unitOfWork, _notifications,
            Mock.Of<ILogger<ReopenFloorballMatchHandler>>())
            .Handle(new ReopenFloorballMatchCommand(_match.Id), CancellationToken.None);
        result.IsSuccess.Should().BeTrue(result.Error);
    }

    private Task<Result> DeleteMatchAsync() =>
        new DeleteFloorballMatchHandler(
            _matches.Object, _players.Object, _statistics.Object, _timer.Object, _unitOfWork,
            Mock.Of<ILogger<DeleteFloorballMatchHandler>>())
            .Handle(new DeleteFloorballMatchCommand(_match.Id), CancellationToken.None);

    private void SetupStatisticsRows()
    {
        _statistics.Setup(r => r.GetGoalieSeasonStatisticsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid playerId, Guid _, Guid _, CancellationToken _) => _goalieRows.GetValueOrDefault(playerId));
        _statistics.Setup(r => r.SaveGoalieSeasonStatisticsAsync(
                It.IsAny<FloorballGoalieSeasonStatistics>(), It.IsAny<CancellationToken>()))
            .Callback((FloorballGoalieSeasonStatistics row, CancellationToken _) => _goalieRows[row.PlayerId] = row)
            .Returns(Task.CompletedTask);
        _statistics.Setup(r => r.GetPlayerSeasonStatisticsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid playerId, Guid _, Guid _, CancellationToken _) => _playerRows.GetValueOrDefault(playerId));
        _statistics.Setup(r => r.SavePlayerSeasonStatisticsAsync(
                It.IsAny<FloorballPlayerSeasonStatistics>(), It.IsAny<CancellationToken>()))
            .Callback((FloorballPlayerSeasonStatistics row, CancellationToken _) => _playerRows[row.PlayerId] = row)
            .Returns(Task.CompletedTask);
        _statistics.Setup(r => r.GetTeamSeasonStatisticsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid teamId, Guid _, CancellationToken _) => _teamRows.GetValueOrDefault(teamId));
        _statistics.Setup(r => r.SaveTeamSeasonStatisticsAsync(
                It.IsAny<FloorballTeamSeasonStatistics>(), It.IsAny<CancellationToken>()))
            .Callback((FloorballTeamSeasonStatistics row, CancellationToken _) => _teamRows[row.TeamId] = row)
            .Returns(Task.CompletedTask);
        _statistics.Setup(r => r.GetMatchTeamStatisticsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, Guid teamId, CancellationToken _) => _matchTeamRows.GetValueOrDefault(teamId));
        _statistics.Setup(r => r.SaveMatchTeamStatisticsAsync(
                It.IsAny<FloorballMatchTeamStatistics>(), It.IsAny<CancellationToken>()))
            .Callback((FloorballMatchTeamStatistics row, CancellationToken _) => _matchTeamRows[row.TeamId] = row)
            .Returns(Task.CompletedTask);
    }
}
