using Application.Common;
using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Handlers;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class FloorballTournamentHandlerTests
{
    private readonly Mock<IFloorballTournamentRepository> _tournamentRepo = new();
    private readonly Mock<IFloorballUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task CreateFloorballTournament_ValidCommand_AddsAndSaves()
    {
        CreateFloorballTournamentHandler handler = new(
            _tournamentRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateFloorballTournamentHandler>>());

        CreateFloorballTournamentCommand command = new(
            Name: "Spring Cup",
            StartDate: new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Unspecified),
            EndDate: new DateTime(2027, 3, 5, 0, 0, 0, DateTimeKind.Unspecified),
            Venue: "Arena",
            ContentHtml: null,
            GroupStageNumberOfPeriods: 2,
            GroupStagePeriodDurationMinutes: 15,
            GroupStageAllowOvertime: true,
            GroupStageOvertimeDurationMinutes: 5,
            GroupStageAllowShootout: true,
            PlayoffNumberOfPeriods: 2,
            PlayoffPeriodDurationMinutes: 15,
            PlayoffAllowOvertime: true,
            PlayoffOvertimeDurationMinutes: 5,
            PlayoffAllowShootout: true,
            TeamsAdvancingPerGroup: 2,
            HasPlayoffStage: true,
            HasThirdPlaceMatch: false);

        Result<FloorballTournamentDto> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Spring Cup");
        result.Data.StartDate.Kind.Should().Be(DateTimeKind.Utc);
        _tournamentRepo.Verify(r => r.AddAsync(It.IsAny<FloorballTournament>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartTournamentGroupStage_WhenMissing_ReturnsNotFound()
    {
        StartTournamentGroupStageHandler handler = new(
            _tournamentRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<StartTournamentGroupStageHandler>>());

        Guid id = Guid.NewGuid();
        _tournamentRepo
            .Setup(r => r.GetByIdWithGroupsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FloorballTournament?)null);

        Result<FloorballTournamentDto> result = await handler.Handle(
            new StartTournamentGroupStageCommand(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task CancelTournament_Valid_CancelsAndSaves()
    {
        CancelTournamentHandler handler = new(
            _tournamentRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CancelTournamentHandler>>());

        FloorballTournament tournament = new(
            "Cup",
            new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 3, 5, 0, 0, 0, DateTimeKind.Utc),
            venue: "Arena");

        _tournamentRepo
            .Setup(r => r.GetByIdWithGroupsAsync(tournament.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        Result<FloorballTournamentDto> result = await handler.Handle(
            new CancelTournamentCommand(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tournament.TournamentStatus.Should().Be(FloorballTournamentStatus.Cancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
