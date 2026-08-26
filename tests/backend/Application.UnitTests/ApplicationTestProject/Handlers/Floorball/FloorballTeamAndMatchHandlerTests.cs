using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Handlers;
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Teams.Handlers;
using Application.Features.Common.MatchTimer.Services;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class FloorballTeamAndMatchHandlerTests
{
    [Fact]
    public async Task CreateFloorballTeam_WhenClubMissing_ReturnsNotFound()
    {
        Mock<IFloorballTeamRepository> teamRepo = new();
        Mock<IClubRepository> clubRepo = new();
        Mock<IFloorballUnitOfWork> uow = new();
        CreateFloorballTeamHandler handler = new(
            teamRepo.Object,
            clubRepo.Object,
            uow.Object,
            Mock.Of<ILogger<CreateFloorballTeamHandler>>());

        Guid clubId = Guid.NewGuid();
        clubRepo.Setup(r => r.GetByIdAsync(clubId)).ReturnsAsync((Club?)null);

        Result<FloorballTeamDto> result = await handler.Handle(
            new CreateFloorballTeamCommand(
                "Wolves",
                DivisionId: null,
                ClubId: clubId,
                HomeArena: "Arena",
                PrimaryJerseyColor: "Blue",
                TeamCategory: TeamCategory.Adult,
                SecondaryJerseyColor: null,
                ShortName: null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Club");
        teamRepo.Verify(r => r.AddAsync(It.IsAny<FloorballTeam>()), Times.Never);
    }

    [Fact]
    public async Task CreateFloorballTeam_Valid_AddsAndSaves()
    {
        Mock<IFloorballTeamRepository> teamRepo = new();
        Mock<IClubRepository> clubRepo = new();
        Mock<IFloorballUnitOfWork> uow = new();
        CreateFloorballTeamHandler handler = new(
            teamRepo.Object,
            clubRepo.Object,
            uow.Object,
            Mock.Of<ILogger<CreateFloorballTeamHandler>>());

        Club club = new("Test Club");
        clubRepo.Setup(r => r.GetByIdAsync(club.Id)).ReturnsAsync(club);

        Result<FloorballTeamDto> result = await handler.Handle(
            new CreateFloorballTeamCommand(
                "Wolves",
                DivisionId: null,
                ClubId: club.Id,
                HomeArena: "Arena",
                PrimaryJerseyColor: "Blue",
                TeamCategory: TeamCategory.Adult,
                SecondaryJerseyColor: null,
                ShortName: null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Wolves");
        teamRepo.Verify(r => r.AddAsync(It.IsAny<FloorballTeam>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartFloorballMatch_WhenMissing_ReturnsNotFound()
    {
        Mock<IFloorballMatchRepository> matchRepo = new();
        Mock<IFloorballUnitOfWork> uow = new();
        Mock<INotificationSenderService> notifications = new();
        Mock<IMatchTimerService> timer = new();
        StartFloorballMatchHandler handler = new(
            matchRepo.Object,
            uow.Object,
            notifications.Object,
            timer.Object,
            Mock.Of<ILogger<StartFloorballMatchHandler>>());

        Guid id = Guid.NewGuid();
        matchRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((FloorballMatch?)null);

        Result<FloorballMatchDto> result = await handler.Handle(
            new StartFloorballMatchCommand(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("FloorballMatch");
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartFloorballMatch_WhenReady_StartsTimerAndNotifies()
    {
        Mock<IFloorballMatchRepository> matchRepo = new();
        Mock<IFloorballUnitOfWork> uow = new();
        Mock<INotificationSenderService> notifications = new();
        Mock<IMatchTimerService> timer = new();
        StartFloorballMatchHandler handler = new(
            matchRepo.Object,
            uow.Object,
            notifications.Object,
            timer.Object,
            Mock.Of<ILogger<StartFloorballMatchHandler>>());

        FloorballSeason season = new(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        Club homeClub = new("Home Club");
        Club awayClub = new("Away Club");
        FloorballTeam home = new("Home", null, homeClub, "Arena", "Blue", TeamCategory.Adult);
        FloorballTeam away = new("Away", null, awayClub, "Arena", "Red", TeamCategory.Adult);
        FloorballPlayer homeGoalie = new(Guid.NewGuid(), new Position(FloorballPosition.Goalkeeper));
        FloorballPlayer awayGoalie = new(Guid.NewGuid(), new Position(FloorballPosition.Goalkeeper));
        home.AddPlayer(homeGoalie, FloorballPosition.Goalkeeper, 1);
        away.AddPlayer(awayGoalie, FloorballPosition.Goalkeeper, 1);
        season.AddTeam(home);
        season.AddTeam(away);

        FloorballMatch match = new(
            season,
            home,
            away,
            new DateTime(2027, 1, 15, 18, 0, 0, DateTimeKind.Utc),
            "Arena");
        Person refPerson = new("Ref", "One");
        match.AddOfficial(new FloorballReferee(
            refPerson.Id,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        match.SetActiveGoalie(home.Id, homeGoalie.Id);
        match.SetActiveGoalie(away.Id, awayGoalie.Id);

        matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        timer.Setup(t => t.ExistsAsync(match.Id)).ReturnsAsync(false);
        timer.Setup(t => t.CreateTimerAsync(match.Id)).Returns(Task.CompletedTask);
        timer.Setup(t => t.StartTimerAsync(match.Id, 1)).Returns(Task.CompletedTask);

        Result<FloorballMatchDto> result = await handler.Handle(
            new StartFloorballMatchCommand(match.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        match.Status.Should().Be(FloorballMatchStatus.InProgress);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        timer.Verify(t => t.CreateTimerAsync(match.Id), Times.Once);
        timer.Verify(t => t.StartTimerAsync(match.Id, 1), Times.Once);
        notifications.Verify(
            n => n.SendNotificationAsync(It.IsAny<string>(), It.IsAny<object>()),
            Times.Once);
    }
}
