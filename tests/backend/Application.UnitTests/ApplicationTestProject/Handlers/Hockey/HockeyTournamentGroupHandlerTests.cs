using Application.Common;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.DTOs;
using Application.Features.Hockey.Tournaments.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class CreateHockeyTournamentGroupHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<CreateHockeyTournamentGroupHandler>> _logger = new();
    private readonly CreateHockeyTournamentGroupHandler _handler;

    public CreateHockeyTournamentGroupHandlerTests()
    {
        _handler = new CreateHockeyTournamentGroupHandler(
            _competitionRepo.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ValidTournament_AddsGroupAndSaves()
    {
        HockeyTournament tournament = new(
            "Christmas Cup",
            new DateTime(2026, 12, 20, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 28, 0, 0, 0, DateTimeKind.Utc),
            "Nokia Arena");

        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        Result<HockeyTournamentDto> result = await _handler.Handle(
            new CreateHockeyTournamentGroupCommand(tournament.Id, "A-lohko"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Groups.Should().ContainSingle(g => g.Name == "A-lohko");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingTournament_ReturnsFailure()
    {
        Guid missingId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(missingId)).ReturnsAsync((HockeyTournament?)null);

        Result<HockeyTournamentDto> result = await _handler.Handle(
            new CreateHockeyTournamentGroupCommand(missingId, "A-lohko"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class AddTeamToHockeyTournamentGroupHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<AddTeamToHockeyTournamentGroupHandler>> _logger = new();
    private readonly AddTeamToHockeyTournamentGroupHandler _handler;

    public AddTeamToHockeyTournamentGroupHandlerTests()
    {
        _handler = new AddTeamToHockeyTournamentGroupHandler(
            _competitionRepo.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ValidCompetitionTeam_AddsToGroupAndSaves()
    {
        HockeyTournament tournament = new(
            "Christmas Cup",
            new DateTime(2026, 12, 20, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 28, 0, 0, 0, DateTimeKind.Utc),
            "Nokia Arena");
        Club club = new("Tappara HC");
        HockeyTeam team = new("Tappara", club, TeamCategory.Adult);
        HockeyCompetitionTeam competitionTeam = tournament.AddTeam(team.Id, seed: 1);
        HockeyTournamentGroup group = tournament.AddGroup("A-lohko");

        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        Result<HockeyTournamentDto> result = await _handler.Handle(
            new AddTeamToHockeyTournamentGroupCommand(tournament.Id, group.Id, competitionTeam.Id, Seed: 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Groups.Should().ContainSingle(g =>
            g.Id == group.Id
            && g.Teams.Count == 1
            && g.Teams.Any(t => t.CompetitionTeamId == competitionTeam.Id && t.Seed == 2));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingTournament_ReturnsFailure()
    {
        Guid missingId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(missingId)).ReturnsAsync((HockeyTournament?)null);

        Result<HockeyTournamentDto> result = await _handler.Handle(
            new AddTeamToHockeyTournamentGroupCommand(missingId, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TeamNotInCompetition_ReturnsFailure()
    {
        HockeyTournament tournament = new(
            "Christmas Cup",
            new DateTime(2026, 12, 20, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 28, 0, 0, 0, DateTimeKind.Utc));
        HockeyTournamentGroup group = tournament.AddGroup("A-lohko");
        Guid unknownCompetitionTeamId = Guid.NewGuid();

        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        Result<HockeyTournamentDto> result = await _handler.Handle(
            new AddTeamToHockeyTournamentGroupCommand(tournament.Id, group.Id, unknownCompetitionTeamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("participating");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
