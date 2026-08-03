using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Competitions.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class AddTeamToHockeyCompetitionHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyTeamRepository> _teamRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<AddTeamToHockeyCompetitionHandler>> _logger = new();
    private readonly AddTeamToHockeyCompetitionHandler _handler;

    public AddTeamToHockeyCompetitionHandlerTests()
    {
        _handler = new AddTeamToHockeyCompetitionHandler(
            _competitionRepo.Object,
            _teamRepo.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ValidTeamAndSeason_AddsCompetitionTeamAndSaves()
    {
        HockeySeason season = new(
            "Test Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc));
        Club club = new("Tappara HC");
        HockeyTeam team = new("Tappara", club, TeamCategory.Adult);

        _teamRepo.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);

        AddTeamToHockeyCompetitionCommand command = new(season.Id, team.Id, Seed: 1);

        Result<HockeyCompetitionTeamDto> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TeamId.Should().Be(team.Id);
        result.Data.CompetitionId.Should().Be(season.Id);
        result.Data.Seed.Should().Be(1);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingTeam_ReturnsFailure()
    {
        HockeySeason season = new(
            "Test Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc));
        Guid missingTeamId = Guid.NewGuid();

        _teamRepo.Setup(r => r.GetByIdAsync(missingTeamId)).ReturnsAsync((HockeyTeam?)null);
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);

        Result<HockeyCompetitionTeamDto> result = await _handler.Handle(
            new AddTeamToHockeyCompetitionCommand(season.Id, missingTeamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("team not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
