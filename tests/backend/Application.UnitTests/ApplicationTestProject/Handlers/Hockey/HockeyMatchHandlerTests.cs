using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Handlers;
using Application.Features.Hockey.Matches.Queries;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Teams;
using Domain.Enums.Hockey.Matches;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class HockeyMatchHandlerTests
{
    private readonly Mock<IHockeyMatchRepository> _matchRepo = new();
    private readonly Mock<IHockeyTeamRepository> _teamRepo = new();
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    private static HockeyMatch CreateStandaloneMatch() =>
        new(
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.Friendly,
            venue: "Nokia Arena");

    [Fact]
    public async Task Create_StandaloneMatch_AddsAndSaves()
    {
        CreateHockeyMatchHandler handler = new(
            _matchRepo.Object,
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyMatchHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new CreateHockeyMatchCommand(
                new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
                HockeyMatchType.Friendly,
                Venue: "Nokia Arena"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.MatchType.Should().Be(HockeyMatchType.Friendly.ToString());
        result.Data.Venue.Should().Be("Nokia Arena");
        _matchRepo.Verify(r => r.AddAsync(It.IsAny<HockeyMatch>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_CompetitionNotFound_ReturnsNotFound()
    {
        Guid missingId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((Domain.Entities.Hockey.Competitions.HockeyCompetition?)null);

        CreateHockeyMatchHandler handler = new(
            _matchRepo.Object,
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyMatchHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new CreateHockeyMatchCommand(
                new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
                HockeyMatchType.League,
                CompetitionId: missingId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddHomeAway_Standalone_AssignsBothSides()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _teamRepo.Setup(r => r.GetByIdAsync(home.Id)).ReturnsAsync(home);
        _teamRepo.Setup(r => r.GetByIdAsync(away.Id)).ReturnsAsync(away);

        AddHomeAwayTeamsToHockeyMatchHandler handler = new(
            _matchRepo.Object,
            _teamRepo.Object,
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddHomeAwayTeamsToHockeyMatchHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new AddHomeAwayTeamsToHockeyMatchCommand(match.Id, home.Id, away.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.HomeTeamId.Should().Be(home.Id);
        result.Data.AwayTeamId.Should().Be(away.Id);
        result.Data.MatchTeams.Should().HaveCount(2);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsDto()
    {
        HockeyMatch match = CreateStandaloneMatch();
        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        GetHockeyMatchByIdHandler handler = new(
            _matchRepo.Object,
            Mock.Of<ILogger<GetHockeyMatchByIdHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new GetHockeyMatchByIdQuery(match.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(match.Id);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        Guid missingId = Guid.NewGuid();
        _matchRepo.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((HockeyMatch?)null);

        GetHockeyMatchByIdHandler handler = new(
            _matchRepo.Object,
            Mock.Of<ILogger<GetHockeyMatchByIdHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new GetHockeyMatchByIdQuery(missingId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ConfirmRoster_ValidPlayers_Confirms()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam team = new("Tappara", club, TeamCategory.Adult);
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer teamPlayer = team.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 12);

        match.AssignMatchTeam(team.Id, HockeyTeamSlot.Home);
        HockeyMatchTeam matchTeam = match.HomeMatchTeam!;

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        ConfirmHockeyMatchRosterHandler handler = new(
            _matchRepo.Object,
            _teamRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<ConfirmHockeyMatchRosterHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new ConfirmHockeyMatchRosterCommand(match.Id, matchTeam.Id, new[] { teamPlayer.Id }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.MatchTeams.Should().ContainSingle(t => t.Id == matchTeam.Id && t.IsConfirmedRoster);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordGoal_WithDressedScorer_IncrementsScore()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer teamPlayer = home.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 12);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        var selection = homeSide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer active = selection.AddActivePlayer(teamPlayer);
        selection.Confirm();

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        RecordHockeyGoalHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RecordHockeyGoalHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new RecordHockeyGoalCommand(
                match.Id,
                homeSide.Id,
                active.Id,
                PeriodNumber: 1,
                TimeInSeconds: 125,
                HockeyGoalStrength.EvenStrength),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.HomeScore.Should().Be(1);
        result.Data.Events.Should().ContainSingle(e => e.EventType == HockeyMatchEventType.Goal.ToString());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkStarted_ScheduledMatch_SetsInProgress()
    {
        HockeyMatch match = CreateStandaloneMatch();
        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        MarkHockeyMatchStartedHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<MarkHockeyMatchStartedHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new MarkHockeyMatchStartedCommand(match.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(HockeyMatchStatus.InProgress.ToString());
        result.Data.ActualStartTime.Should().NotBeNull();
        result.Data.CurrentPeriodNumber.Should().Be(1);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddOfficial_AssignsOfficial()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Guid officialId = Guid.NewGuid();
        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        AddHockeyMatchOfficialHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddHockeyMatchOfficialHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new AddHockeyMatchOfficialCommand(match.Id, officialId, HockeyOfficialRole.Referee, IsMainOfficial: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Officials.Should().ContainSingle(o => o.OfficialId == officialId && o.IsMainOfficial);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordFaceoff_WithTeams_AddsEvent()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        HockeyMatchTeam awaySide = match.AwayMatchTeam!;

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        RecordHockeyFaceoffHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RecordHockeyFaceoffHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new RecordHockeyFaceoffCommand(
                match.Id,
                homeSide.Id,
                awaySide.Id,
                PeriodNumber: 1,
                TimeInSeconds: 0,
                HockeyFaceoffZone.NeutralZone,
                HockeyFaceoffSpot.CenterIce),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Events.Should().ContainSingle(e => e.EventType == HockeyMatchEventType.Faceoff.ToString());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMatchLine_AddsLineToSide()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        AddHockeyMatchLineHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddHockeyMatchLineHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new AddHockeyMatchLineCommand(match.Id, homeSide.Id, "PP1", HockeyLineType.PowerPlayUnit, LineNumber: 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.MatchTeams.Should().ContainSingle(t => t.Id == homeSide.Id)
            .Which.Lines.Should().ContainSingle(l =>
                l.Name == "PP1" && l.LineType == HockeyLineType.PowerPlayUnit.ToString());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnableOnIceAndAddPlayer_TracksPlayer()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer teamPlayer = home.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 12);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        var selection = homeSide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer active = selection.AddActivePlayer(teamPlayer);
        selection.Confirm();

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        EnableHockeyMatchOnIceTrackingHandler enableHandler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<EnableHockeyMatchOnIceTrackingHandler>>());

        Result<HockeyMatchDto> enableResult = await enableHandler.Handle(
            new EnableHockeyMatchOnIceTrackingCommand(match.Id, homeSide.Id),
            CancellationToken.None);
        enableResult.IsSuccess.Should().BeTrue();
        enableResult.Data!.MatchTeams.Single(t => t.Id == homeSide.Id).TracksOnIcePlayers.Should().BeTrue();

        AddHockeyMatchPlayerToIceHandler addHandler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddHockeyMatchPlayerToIceHandler>>());

        Result<HockeyMatchDto> addResult = await addHandler.Handle(
            new AddHockeyMatchPlayerToIceCommand(match.Id, homeSide.Id, active.Id),
            CancellationToken.None);

        addResult.IsSuccess.Should().BeTrue();
        addResult.Data!.MatchTeams.Single(t => t.Id == homeSide.Id).OnIceState!.PlayersOnIce
            .Should().ContainSingle(p => p.MatchActivePlayerId == active.Id);
    }

    [Fact]
    public async Task DeactivateRosterPlayer_MarksInactive()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer teamPlayer = home.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 12);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        var selection = homeSide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer active = selection.AddActivePlayer(teamPlayer);
        selection.Confirm();

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        DeactivateHockeyMatchRosterPlayerHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeactivateHockeyMatchRosterPlayerHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new DeactivateHockeyMatchRosterPlayerCommand(match.Id, homeSide.Id, active.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.MatchTeams.Single(t => t.Id == homeSide.Id).ActivePlayers
            .Should().ContainSingle(p => p.Id == active.Id && !p.IsActive);
        result.Data.MatchTeams.Single(t => t.Id == homeSide.Id).IsConfirmedRoster.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteGoal_ExistingGoal_DecrementsScoreAndMarksDeleted()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer teamPlayer = home.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 12);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        var selection = homeSide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer active = selection.AddActivePlayer(teamPlayer);
        selection.Confirm();
        match.MarkStarted();

        HockeyGoal goal = new(
            match.Id,
            homeSide.Id,
            active.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(60),
            HockeyGoalStrength.EvenStrength);
        match.AddEvent(goal);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        DeleteHockeyGoalHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteHockeyGoalHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new DeleteHockeyGoalCommand(match.Id, goal.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.HomeScore.Should().Be(0);
        result.Data.Events.Should().BeEmpty();
        _matchRepo.Verify(r => r.MarkEventAsDeleted(goal), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteGoal_FinishedMatch_ReturnsFailure()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer teamPlayer = home.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 12);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        var selection = homeSide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer active = selection.AddActivePlayer(teamPlayer);
        selection.Confirm();
        match.MarkStarted();

        HockeyGoal goal = new(
            match.Id,
            homeSide.Id,
            active.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(60),
            HockeyGoalStrength.EvenStrength);
        match.AddEvent(goal);
        match.MarkFinished(resultType: HockeyMatchResultType.HomeWin);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        DeleteHockeyGoalHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteHockeyGoalHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new DeleteHockeyGoalCommand(match.Id, goal.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Finished");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteGoal_MissingEvent_ReturnsFailure()
    {
        HockeyMatch match = CreateStandaloneMatch();
        match.MarkStarted();
        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        DeleteHockeyGoalHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteHockeyGoalHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new DeleteHockeyGoalCommand(match.Id, Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeletePenalty_Existing_RemovesEvent()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        match.MarkStarted();

        HockeyPenalty penalty = new(
            match.Id,
            homeSide.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(30),
            HockeyPenaltySeverity.Minor,
            HockeyPenaltyOffence.Hooking,
            penaltyMinutes: 2);
        match.AddEvent(penalty);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        DeleteHockeyPenaltyHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteHockeyPenaltyHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new DeleteHockeyPenaltyCommand(match.Id, penalty.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Events.Should().BeEmpty();
        _matchRepo.Verify(r => r.MarkEventAsDeleted(penalty), Times.Once);
    }

    [Fact]
    public async Task DeleteShot_Existing_RemovesEvent()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        match.MarkStarted();

        HockeyShot shot = new(
            match.Id,
            homeSide.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(45),
            HockeyShotResult.Saved,
            countsAsShotOnGoal: true);
        match.AddEvent(shot);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        DeleteHockeyShotHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteHockeyShotHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new DeleteHockeyShotCommand(match.Id, shot.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Events.Should().BeEmpty();
        _matchRepo.Verify(r => r.MarkEventAsDeleted(shot), Times.Once);
    }

    [Fact]
    public async Task UpdateGoal_Existing_UpdatesDetailsAndKeepsScore()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer teamPlayer = home.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 12);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        var selection = homeSide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer active = selection.AddActivePlayer(teamPlayer);
        selection.Confirm();
        match.MarkStarted();

        HockeyGoal goal = new(
            match.Id,
            homeSide.Id,
            active.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(60),
            HockeyGoalStrength.EvenStrength);
        match.AddEvent(goal);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        UpdateHockeyGoalHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyGoalHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new UpdateHockeyGoalCommand(
                match.Id,
                goal.Id,
                homeSide.Id,
                active.Id,
                PeriodNumber: 2,
                TimeInSeconds: 125,
                HockeyGoalStrength.PowerPlayOneMan,
                WasEmptyNet: true,
                Description: "Corrected time"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.HomeScore.Should().Be(1);
        result.Data.AwayScore.Should().Be(0);
        HockeyMatchEventDto updated = result.Data.Events.Should().ContainSingle().Subject;
        updated.PeriodNumber.Should().Be(2);
        updated.GameTimeSeconds.Should().Be(125);
        updated.Description.Should().Be("Corrected time");
        goal.GoalStrength.Should().Be(HockeyGoalStrength.PowerPlayOneMan);
        goal.WasEmptyNet.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoal_ChangeScoringTeam_MovesScoreboard()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);
        HockeyPlayer homePlayer = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyPlayer awayPlayer = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer homeTeamPlayer = home.AddPlayer(homePlayer, HockeyPosition.Center, jerseyNumber: 12);
        HockeyTeamPlayer awayTeamPlayer = away.AddPlayer(awayPlayer, HockeyPosition.Center, jerseyNumber: 91);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        HockeyMatchTeam awaySide = match.AwayMatchTeam!;

        var homeSelection = homeSide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer homeActive = homeSelection.AddActivePlayer(homeTeamPlayer);
        homeSelection.Confirm();

        var awaySelection = awaySide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer awayActive = awaySelection.AddActivePlayer(awayTeamPlayer);
        awaySelection.Confirm();

        match.MarkStarted();

        HockeyGoal goal = new(
            match.Id,
            homeSide.Id,
            homeActive.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(60),
            HockeyGoalStrength.EvenStrength);
        match.AddEvent(goal);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        UpdateHockeyGoalHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyGoalHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new UpdateHockeyGoalCommand(
                match.Id,
                goal.Id,
                awaySide.Id,
                awayActive.Id,
                PeriodNumber: 1,
                TimeInSeconds: 60,
                HockeyGoalStrength.EvenStrength),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.HomeScore.Should().Be(0);
        result.Data.AwayScore.Should().Be(1);
        goal.ScoringMatchTeamId.Should().Be(awaySide.Id);
        goal.ScorerActivePlayerId.Should().Be(awayActive.Id);
    }

    [Fact]
    public async Task UpdateGoal_FinishedMatch_ReturnsFailure()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);
        HockeyPlayer player = new(Guid.NewGuid(), HockeyPosition.Center);
        HockeyTeamPlayer teamPlayer = home.AddPlayer(player, HockeyPosition.Center, jerseyNumber: 12);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        var selection = homeSide.CreateOrReplacePlayerSelection(HockeyPlayerSelectionSource.Manual);
        HockeyMatchActivePlayer active = selection.AddActivePlayer(teamPlayer);
        selection.Confirm();
        match.MarkStarted();

        HockeyGoal goal = new(
            match.Id,
            homeSide.Id,
            active.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(60),
            HockeyGoalStrength.EvenStrength);
        match.AddEvent(goal);
        match.MarkFinished(resultType: HockeyMatchResultType.HomeWin);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        UpdateHockeyGoalHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyGoalHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new UpdateHockeyGoalCommand(
                match.Id,
                goal.Id,
                homeSide.Id,
                active.Id,
                PeriodNumber: 2,
                TimeInSeconds: 90,
                HockeyGoalStrength.EvenStrength),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Finished");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePenalty_Existing_UpdatesDetails()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        match.MarkStarted();

        HockeyPenalty penalty = new(
            match.Id,
            homeSide.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(30),
            HockeyPenaltySeverity.Minor,
            HockeyPenaltyOffence.Hooking,
            penaltyMinutes: 2);
        match.AddEvent(penalty);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        UpdateHockeyPenaltyHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyPenaltyHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new UpdateHockeyPenaltyCommand(
                match.Id,
                penalty.Id,
                homeSide.Id,
                PeriodNumber: 2,
                TimeInSeconds: 200,
                HockeyPenaltySeverity.Major,
                HockeyPenaltyOffence.Fighting,
                PenaltyMinutes: 5,
                Description: "Corrected"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        HockeyMatchEventDto updated = result.Data!.Events.Should().ContainSingle().Subject;
        updated.PeriodNumber.Should().Be(2);
        updated.GameTimeSeconds.Should().Be(200);
        penalty.Severity.Should().Be(HockeyPenaltySeverity.Major);
        penalty.Offence.Should().Be(HockeyPenaltyOffence.Fighting);
        penalty.PenaltyMinutes.Should().Be(5);
    }

    [Fact]
    public async Task UpdateShot_Existing_UpdatesDetails()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);
        HockeyMatchTeam homeSide = match.HomeMatchTeam!;
        match.MarkStarted();

        HockeyShot shot = new(
            match.Id,
            homeSide.Id,
            periodNumber: 1,
            gameTime: TimeSpan.FromSeconds(45),
            HockeyShotResult.Saved,
            countsAsShotOnGoal: true);
        match.AddEvent(shot);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

        UpdateHockeyShotHandler handler = new(
            _matchRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeyShotHandler>>());

        Result<HockeyMatchDto> result = await handler.Handle(
            new UpdateHockeyShotCommand(
                match.Id,
                shot.Id,
                homeSide.Id,
                PeriodNumber: 3,
                TimeInSeconds: 500,
                HockeyShotResult.Missed,
                CountsAsShotOnGoal: false,
                Description: "Was a miss"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        HockeyMatchEventDto updated = result.Data!.Events.Should().ContainSingle().Subject;
        updated.PeriodNumber.Should().Be(3);
        updated.GameTimeSeconds.Should().Be(500);
        shot.ShotResult.Should().Be(HockeyShotResult.Missed);
        shot.CountsAsShotOnGoal.Should().BeFalse();
    }

    [Fact]
    public async Task GetByCompetition_ReturnsMappedMatches()
    {
        Guid competitionId = Guid.NewGuid();
        HockeyMatch match = new(
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.League,
            competitionId: competitionId,
            venue: "Arena");

        _matchRepo
            .Setup(r => r.GetByCompetitionIdAsync(competitionId))
            .ReturnsAsync(new List<HockeyMatch> { match });

        GetHockeyMatchesByCompetitionHandler handler = new(
            _matchRepo.Object,
            Mock.Of<ILogger<GetHockeyMatchesByCompetitionHandler>>());

        Result<IEnumerable<HockeyMatchDto>> result = await handler.Handle(
            new GetHockeyMatchesByCompetitionQuery(competitionId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle(m => m.Id == match.Id);
    }

    [Fact]
    public async Task GetByTeam_ReturnsMappedMatches()
    {
        Guid teamId = Guid.NewGuid();
        HockeyMatch match = CreateStandaloneMatch();

        _matchRepo
            .Setup(r => r.GetByTeamIdAsync(teamId))
            .ReturnsAsync(new List<HockeyMatch> { match });

        GetHockeyMatchesByTeamHandler handler = new(
            _matchRepo.Object,
            Mock.Of<ILogger<GetHockeyMatchesByTeamHandler>>());

        Result<IEnumerable<HockeyMatchDto>> result = await handler.Handle(
            new GetHockeyMatchesByTeamQuery(teamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle(m => m.Id == match.Id);
    }

    [Fact]
    public async Task GetByCompetition_Empty_ReturnsEmptyList()
    {
        Guid competitionId = Guid.NewGuid();
        _matchRepo
            .Setup(r => r.GetByCompetitionIdAsync(competitionId))
            .ReturnsAsync(Array.Empty<HockeyMatch>());

        GetHockeyMatchesByCompetitionHandler handler = new(
            _matchRepo.Object,
            Mock.Of<ILogger<GetHockeyMatchesByCompetitionHandler>>());

        Result<IEnumerable<HockeyMatchDto>> result = await handler.Handle(
            new GetHockeyMatchesByCompetitionQuery(competitionId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
