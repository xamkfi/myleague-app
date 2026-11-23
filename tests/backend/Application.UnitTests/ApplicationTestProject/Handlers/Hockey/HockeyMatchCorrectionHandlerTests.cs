using Application.Common;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Handlers;
using Application.Features.Hockey.Statistics.Commands;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

/// <summary>
/// Deleting matches, missed penalty shots, delayed-penalty goals and correcting how a match was decided.
/// </summary>
public class HockeyMatchCorrectionHandlerTests
{
    private readonly Mock<IHockeyMatchRepository> _matches = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IMatchTimerService> _timer = new();

    private readonly HockeyMatch _match;
    private readonly HockeyMatchTeam _home;
    private readonly HockeyMatchActivePlayer _skater;

    public HockeyMatchCorrectionHandlerTests()
    {
        _match = new HockeyMatch(
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc), HockeyMatchType.Friendly, venue: "Arena");
        Club club = new("Club");
        HockeyTeam home = new("Home", club, TeamCategory.Adult);
        HockeyTeam away = new("Away", club, TeamCategory.Adult);
        HockeyTeamPlayer teamPlayer = home.AddPlayer(
            new HockeyPlayer(Guid.NewGuid(), HockeyPosition.Center), HockeyPosition.Center, jerseyNumber: 12);
        _match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        _match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        _home = _match.HomeMatchTeam!;
        _skater = _home.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual).AddActivePlayer(teamPlayer);

        _matches.Setup(r => r.GetByIdAsync(_match.Id)).ReturnsAsync(_match);
        _matches.Setup(r => r.DeleteMatchAsync(_match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mediator
            .Setup(m => m.Send(It.IsAny<RecalculateHockeyCompetitionScopesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    [Fact]
    public async Task DeleteMatch_Scheduled_DeletesWithoutTouchingTimer()
    {
        Result result = await DeleteAsync();

        result.IsSuccess.Should().BeTrue(result.Error);
        _matches.Verify(r => r.DeleteMatchAsync(_match.Id, It.IsAny<CancellationToken>()), Times.Once);
        _timer.Verify(t => t.DestroyTimerAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMatch_Finished_DeletesMatchAndTimer()
    {
        _match.MarkStarted();
        _match.MarkFinished();

        Result result = await DeleteAsync();

        result.IsSuccess.Should().BeTrue(result.Error);
        _matches.Verify(r => r.DeleteMatchAsync(_match.Id, It.IsAny<CancellationToken>()), Times.Once);
        _timer.Verify(t => t.DestroyTimerAsync(_match.Id), Times.Once);
        _mediator.Verify(
            m => m.Send(It.IsAny<RecalculateHockeyMatchStatisticsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMatch_StartedPlayoffMatch_ReturnsFailure()
    {
        _match.SetPlayoffInfo(HockeyPlayoffRound.Final, 0, null, null);
        _match.MarkStarted();

        Result result = await DeleteAsync();

        result.IsFailure.Should().BeTrue();
        _matches.Verify(r => r.DeleteMatchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMatch_Missing_ReturnsNotFound()
    {
        Result result = await CreateDeleteHandler()
            .Handle(new DeleteHockeyMatchCommand(Guid.NewGuid()), CancellationToken.None);

        result.ErrorKind.Should().Be(ResultErrorKind.NotFound);
    }

    [Fact]
    public async Task RecordMissedPenaltyShot_AddsEventWithoutScore()
    {
        _match.MarkStarted();

        Result<HockeyMatchDto> result = await new RecordHockeyMissedPenaltyShotHandler(_matches.Object, _unitOfWork.Object)
            .Handle(
                new RecordHockeyMissedPenaltyShotCommand(_match.Id, _home.Id, _skater.Id, 2, 1500),
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Data!.HomeScore.Should().Be(0);
        result.Data.Events.Should().ContainSingle(e =>
            e.EventType == nameof(HockeyMatchEventType.MissedPenaltyShot) && e.MatchActivePlayerId == _skater.Id);
        _matches.Verify(r => r.MarkEventAsAdded(It.IsAny<HockeyMissedPenaltyShot>()), Times.Once);
    }

    [Fact]
    public async Task RecordMissedPenaltyShot_UnknownShooter_ReturnsFailureWithoutSaving()
    {
        _match.MarkStarted();

        Result<HockeyMatchDto> result = await new RecordHockeyMissedPenaltyShotHandler(_matches.Object, _unitOfWork.Object)
            .Handle(
                new RecordHockeyMissedPenaltyShotCommand(_match.Id, _home.Id, Guid.NewGuid(), 2, 1500),
                CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordGoal_DelayedPenaltyAndPenaltyShot_AreReturned()
    {
        _match.MarkStarted();

        Result<HockeyMatchDto> result = await new RecordHockeyGoalHandler(
            _matches.Object, _unitOfWork.Object, Mock.Of<ILogger<RecordHockeyGoalHandler>>())
            .Handle(
                new RecordHockeyGoalCommand(
                    _match.Id, _home.Id, _skater.Id, 1, 60, HockeyGoalStrength.EvenStrength,
                    WasDelayedPenalty: true, WasPenaltyShotGoal: true),
                CancellationToken.None);

        HockeyMatchEventDto goal = result.Data!.Events.Should().ContainSingle().Subject;
        goal.WasDelayedPenalty.Should().BeTrue();
        goal.WasPenaltyShotGoal.Should().BeTrue();
        goal.WasEmptyNet.Should().BeFalse();
    }

    [Fact]
    public async Task SetDecision_Shootout_SetsFlag()
    {
        _match.MarkStarted();

        Result<HockeyMatchDto> result = await new SetHockeyMatchDecisionHandler(_matches.Object, _unitOfWork.Object)
            .Handle(new SetHockeyMatchDecisionCommand(_match.Id, MatchDecision.Shootout), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Data!.WentToShootout.Should().BeTrue();
        result.Data.WentToOvertime.Should().BeFalse();
    }

    private DeleteHockeyMatchHandler CreateDeleteHandler() => new(
        _matches.Object, _mediator.Object, _timer.Object, Mock.Of<ILogger<DeleteHockeyMatchHandler>>());

    private Task<Result> DeleteAsync() =>
        CreateDeleteHandler().Handle(new DeleteHockeyMatchCommand(_match.Id), CancellationToken.None);
}
