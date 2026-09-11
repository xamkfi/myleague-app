using Application.Common;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.DTOs;
using Application.Features.Hockey.Tournaments.Handlers;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class HockeyTournamentLifecycleHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Publish_ValidDraft_Saves()
    {
        HockeyTournament tournament = new(
            "Cup",
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc));
        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        PublishHockeyTournamentHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<PublishHockeyTournamentHandler>>());

        Result<HockeyTournamentDto> result = await handler.Handle(
            new PublishHockeyTournamentCommand(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(HockeyCompetitionStatus.Published.ToString());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveGroup_ExistingGroup_RemovesAndSaves()
    {
        HockeyTournament tournament = new(
            "Cup",
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc));
        HockeyTournamentGroup group = tournament.AddGroup("A");
        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        RemoveHockeyTournamentGroupHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RemoveHockeyTournamentGroupHandler>>());

        Result<HockeyTournamentDto> result = await handler.Handle(
            new RemoveHockeyTournamentGroupCommand(tournament.Id, group.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Groups.Should().BeEmpty();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePlayoffSeries_ValidRequest_AddsSeries()
    {
        HockeyTournament tournament = new(
            "Cup",
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc));
        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        CreateHockeyPlayoffSeriesHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeyPlayoffSeriesHandler>>());

        Result<HockeyTournamentDto> result = await handler.Handle(
            new CreateHockeyPlayoffSeriesCommand(tournament.Id, HockeyPlayoffRound.SemiFinal, 0, 3),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PlayoffSeries.Should().ContainSingle(s => s.Round == HockeyPlayoffRound.SemiFinal.ToString() && s.BestOf == 3);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetChampion_WhenNotCompleted_Fails()
    {
        HockeyTournament tournament = new(
            "Cup",
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc));
        Guid competitionTeamId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        SetHockeyTournamentChampionHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<SetHockeyTournamentChampionHandler>>());

        Result<HockeyTournamentDto> result = await handler.Handle(
            new SetHockeyTournamentChampionCommand(tournament.Id, competitionTeamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("completed");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetChampion_WhenCompleted_SetsChampion()
    {
        HockeyTournament tournament = new(
            "Cup",
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc));
        tournament.Publish();
        tournament.Activate();
        tournament.Complete();
        Guid competitionTeamId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetTournamentByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        SetHockeyTournamentChampionHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<SetHockeyTournamentChampionHandler>>());

        Result<HockeyTournamentDto> result = await handler.Handle(
            new SetHockeyTournamentChampionCommand(tournament.Id, competitionTeamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.ChampionCompetitionTeamId.Should().Be(competitionTeamId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
