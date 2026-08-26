using Application.Common;
using Application.Features.Football.Tournaments.Commands;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Handlers;
using Domain.Entities.Football.Competitions;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Football;

public class FootballTournamentHandlerTests
{
    private readonly Mock<IFootballTournamentRepository> _tournamentRepo = new();
    private readonly Mock<IFootballUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task CreateFootballTournament_ValidCommand_AddsAndSaves()
    {
        CreateFootballTournamentHandler handler = new(
            _tournamentRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateFootballTournamentHandler>>());

        CreateFootballTournamentCommand command = new(
            Name: "Football Cup",
            StartDate: new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Unspecified),
            EndDate: new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Unspecified),
            Venue: "Pitch",
            ContentHtml: null,
            GroupStageNumberOfHalves: 2,
            GroupStageHalfDurationMinutes: 20,
            GroupStagePlayersOnField: 5,
            GroupStageRequireGoalkeeper: true,
            GroupStageMaxSubstitutions: 0,
            GroupStageRequireOfficialsToStart: false,
            GroupStageAllowExtraTime: false,
            GroupStageExtraTimeHalfCount: 2,
            GroupStageExtraTimeHalfDurationMinutes: 5,
            GroupStageAllowPenaltyShootout: false,
            PlayoffNumberOfHalves: 2,
            PlayoffHalfDurationMinutes: 20,
            PlayoffPlayersOnField: 5,
            PlayoffRequireGoalkeeper: true,
            PlayoffMaxSubstitutions: 0,
            PlayoffRequireOfficialsToStart: false,
            PlayoffAllowExtraTime: true,
            PlayoffExtraTimeHalfCount: 2,
            PlayoffExtraTimeHalfDurationMinutes: 5,
            PlayoffAllowPenaltyShootout: true,
            TeamsAdvancingPerGroup: 2,
            HasPlayoffStage: true,
            HasThirdPlaceMatch: false);

        Result<FootballTournamentDto> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Football Cup");
        result.Data.StartDate.Kind.Should().Be(DateTimeKind.Utc);
        _tournamentRepo.Verify(r => r.AddAsync(It.IsAny<FootballTournament>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelTournament_Valid_CancelsAndSaves()
    {
        CancelTournamentHandler handler = new(
            _tournamentRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CancelTournamentHandler>>());

        FootballTournament tournament = new(
            "Cup",
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc),
            venue: "Pitch");

        _tournamentRepo
            .Setup(r => r.GetByIdWithGroupsAsync(tournament.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        Result<FootballTournamentDto> result = await handler.Handle(
            new CancelTournamentCommand(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tournament.TournamentStatus.Should().Be(FootballTournamentStatus.Cancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
