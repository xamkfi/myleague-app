using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Competitions.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class HockeyCompetitionDivisionHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IDivisionRepository> _divisionRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    private static HockeySeason CreateSeason() =>
        new(
            "Liiga 2026-27",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            "2026-27");

    [Fact]
    public async Task CreateDivision_ValidIceHockeyDivision_AddsAndSaves()
    {
        HockeySeason season = CreateSeason();
        Division division = new("SM-liiga", "Top tier", 1, SportsCategory.Icehockey);
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _divisionRepo.Setup(r => r.GetByIdAsync(division.Id)).ReturnsAsync(division);

        CreateHockeyCompetitionDivisionHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyCompetitionDivisionHandler>>());

        Result<HockeyCompetitionDto> result = await handler.Handle(
            new CreateHockeyCompetitionDivisionCommand(season.Id, division.Id, "SM-liiga", 0),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Divisions.Should().ContainSingle(d => d.DivisionId == division.Id && d.Name == "SM-liiga");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDivision_DivisionNotFound_ReturnsNotFound()
    {
        HockeySeason season = CreateSeason();
        Guid missingDivisionId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _divisionRepo.Setup(r => r.GetByIdAsync(missingDivisionId)).ReturnsAsync((Division?)null);

        CreateHockeyCompetitionDivisionHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyCompetitionDivisionHandler>>());

        Result<HockeyCompetitionDto> result = await handler.Handle(
            new CreateHockeyCompetitionDivisionCommand(season.Id, missingDivisionId, "Missing", 0),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDivision_WrongSportType_Fails()
    {
        HockeySeason season = CreateSeason();
        Division division = new("Salibandy", "Floorball", 1, SportsCategory.Floorball);
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _divisionRepo.Setup(r => r.GetByIdAsync(division.Id)).ReturnsAsync(division);

        CreateHockeyCompetitionDivisionHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyCompetitionDivisionHandler>>());

        Result<HockeyCompetitionDto> result = await handler.Handle(
            new CreateHockeyCompetitionDivisionCommand(season.Id, division.Id, "Wrong", 0),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("ice hockey");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddTeamToDivision_ValidMembership_AddsTeam()
    {
        HockeySeason season = CreateSeason();
        Guid teamId = Guid.NewGuid();
        HockeyCompetitionTeam competitionTeam = season.AddTeam(teamId);
        HockeyCompetitionDivision division = season.AddDivision(Guid.NewGuid(), "SM-liiga", 0);
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);

        AddTeamToHockeyCompetitionDivisionHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<AddTeamToHockeyCompetitionDivisionHandler>>());

        Result<HockeyCompetitionDto> result = await handler.Handle(
            new AddTeamToHockeyCompetitionDivisionCommand(season.Id, division.Id, competitionTeam.Id, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Divisions.Should().ContainSingle(d =>
            d.Id == division.Id &&
            d.Teams.Any(t => t.CompetitionTeamId == competitionTeam.Id && t.IsActive));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveTeam_ExistingTeam_RemovesAndSaves()
    {
        HockeySeason season = CreateSeason();
        Guid teamId = Guid.NewGuid();
        season.AddTeam(teamId);
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);

        RemoveTeamFromHockeyCompetitionHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RemoveTeamFromHockeyCompetitionHandler>>());

        Result<HockeyCompetitionDto> result = await handler.Handle(
            new RemoveTeamFromHockeyCompetitionCommand(season.Id, teamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Teams.Should().NotContain(t => t.TeamId == teamId && t.IsActive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveTeam_CompetitionNotFound_ReturnsNotFound()
    {
        Guid missingId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((HockeyCompetition?)null);

        RemoveTeamFromHockeyCompetitionHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RemoveTeamFromHockeyCompetitionHandler>>());

        Result<HockeyCompetitionDto> result = await handler.Handle(
            new RemoveTeamFromHockeyCompetitionCommand(missingId, Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
