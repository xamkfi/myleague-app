using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.FloorballMatches;

/// <summary>
/// Tests for the relaxed create-match flow that allows publishing a fixture before its
/// participants are known. The original behavior (both team IDs required) was loosened to
/// support future-scheduled league rounds and playoff slots awaiting their feeders.
/// </summary>
public class CreateFloorballMatchHandlerTests
{
    private readonly Mock<IFloorballMatchRepository> _matchRepo = new();
    private readonly Mock<IFloorballTeamRepository> _teamRepo = new();
    private readonly Mock<IFloorballCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IFloorballRefereeRepository> _refereeRepo = new();
    private readonly Mock<IFloorballUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<CreateFloorballMatchHandler>> _logger = new();

    private readonly CreateFloorballMatchHandler _handler;

    public CreateFloorballMatchHandlerTests()
    {
        _handler = new CreateFloorballMatchHandler(
            _matchRepo.Object,
            _teamRepo.Object,
            _competitionRepo.Object,
            _refereeRepo.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    private static FloorballSeason CreateSeason()
    {
        return new FloorballSeason(
            "Test Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
    }

    private static FloorballTeam CreateTeam(string name)
    {
        Club club = new Club(name + " HC");
        return new FloorballTeam(
            name,
            divisionId: null,
            club,
            homeArena: "Test Arena",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }

    [Fact]
    public async Task Handle_WithBothTeamsNull_CreatesPlaceholderMatch()
    {
        // Arrange: deliberately omit both team IDs to mirror the "publish a placeholder
        // fixture and assign teams later" admin workflow.
        FloorballSeason season = CreateSeason();
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);

        CreateFloorballMatchCommand command = new CreateFloorballMatchCommand(
            CompetitionId: season.Id,
            HomeTeamId: null,
            AwayTeamId: null,
            RefereeId: null,
            ScheduledDateTime: new DateTime(2027, 1, 15, 18, 30, 0, DateTimeKind.Utc),
            Venue: "Arena");

        // Act
        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HomeTeamId.Should().BeNull();
        result.Data.AwayTeamId.Should().BeNull();
        // No team repository lookup should have happened — we don't want a spurious round trip.
        _teamRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid?>()), Times.Never);
        _matchRepo.Verify(r => r.AddAsync(It.IsAny<FloorballMatch>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOnlyHomeTeamProvided_AssignsHomeAndLeavesAwayTbd()
    {
        // Arrange: half-filled fixture (winner of qualifier vs. unnamed opponent).
        FloorballSeason season = CreateSeason();
        FloorballTeam home = CreateTeam("Wolves");
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)home.Id)).ReturnsAsync(home);

        CreateFloorballMatchCommand command = new CreateFloorballMatchCommand(
            CompetitionId: season.Id,
            HomeTeamId: home.Id,
            AwayTeamId: null,
            RefereeId: null,
            ScheduledDateTime: new DateTime(2027, 1, 15, 18, 30, 0, DateTimeKind.Utc),
            Venue: "Arena");

        // Act
        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.HomeTeamId.Should().Be(home.Id);
        result.Data.AwayTeamId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnknownHomeTeamId_ReturnsNotFound()
    {
        // Even after relaxing the "teams required" rule, a *provided* team ID must still
        // resolve — otherwise the admin has typoed and we surface a clean NotFound so the
        // frontend can render an actionable error.
        FloorballSeason season = CreateSeason();
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        Guid missingTeamId = Guid.NewGuid();
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)missingTeamId)).ReturnsAsync((FloorballTeam?)null);

        CreateFloorballMatchCommand command = new CreateFloorballMatchCommand(
            CompetitionId: season.Id,
            HomeTeamId: missingTeamId,
            AwayTeamId: null,
            RefereeId: null,
            ScheduledDateTime: new DateTime(2027, 1, 15, 18, 30, 0, DateTimeKind.Utc),
            Venue: "Arena");

        // Act
        Result<FloorballMatchDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(missingTeamId.ToString());
        _matchRepo.Verify(r => r.AddAsync(It.IsAny<FloorballMatch>()), Times.Never);
    }
}
