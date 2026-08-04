using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Handlers;
using Application.Features.Hockey.Matches.Queries;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;
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
}
