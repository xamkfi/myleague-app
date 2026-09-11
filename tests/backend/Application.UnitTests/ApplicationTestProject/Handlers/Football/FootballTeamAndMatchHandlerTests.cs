using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Handlers;
using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Teams.Handlers;
using Application.Features.Common.MatchTimer.Services;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Football;

public class FootballTeamAndMatchHandlerTests
{
    [Fact]
    public async Task CreateFootballTeam_WhenClubMissing_ReturnsNotFound()
    {
        Mock<IFootballTeamRepository> teamRepo = new();
        Mock<IClubRepository> clubRepo = new();
        Mock<IFootballUnitOfWork> uow = new();
        CreateFootballTeamHandler handler = new(
            teamRepo.Object,
            clubRepo.Object,
            uow.Object,
            Mock.Of<ILogger<CreateFootballTeamHandler>>());

        Guid clubId = Guid.NewGuid();
        clubRepo.Setup(r => r.GetByIdAsync(clubId)).ReturnsAsync((Club?)null);

        Result<FootballTeamDto> result = await handler.Handle(
            new CreateFootballTeamCommand(
                "United",
                DivisionId: null,
                ClubId: clubId,
                HomeArena: "Pitch",
                PrimaryJerseyColor: "Red",
                TeamCategory: TeamCategory.Adult,
                SecondaryJerseyColor: null,
                ShortName: null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Club");
        teamRepo.Verify(r => r.AddAsync(It.IsAny<FootballTeam>()), Times.Never);
    }

    [Fact]
    public async Task StartFootballMatch_WhenMissing_ReturnsNotFound()
    {
        Mock<IFootballMatchRepository> matchRepo = new();
        Mock<IFootballUnitOfWork> uow = new();
        StartFootballMatchHandler handler = new(
            matchRepo.Object,
            uow.Object,
            Mock.Of<INotificationSenderService>(),
            Mock.Of<IMatchTimerService>(),
            Mock.Of<ILogger<StartFootballMatchHandler>>());

        Guid id = Guid.NewGuid();
        matchRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((FootballMatch?)null);

        Result<FootballMatchDto> result = await handler.Handle(
            new StartFootballMatchCommand(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("FootballMatch");
    }

    [Fact]
    public async Task CreateFootballMatch_WithBothTeamsNull_CreatesPlaceholder()
    {
        Mock<IFootballMatchRepository> matchRepo = new();
        Mock<IFootballTeamRepository> teamRepo = new();
        Mock<IFootballCompetitionRepository> competitionRepo = new();
        Mock<IFootballRefereeRepository> refereeRepo = new();
        Mock<IFootballUnitOfWork> uow = new();
        CreateFootballMatchHandler handler = new(
            matchRepo.Object,
            teamRepo.Object,
            competitionRepo.Object,
            refereeRepo.Object,
            uow.Object,
            Mock.Of<ILogger<CreateFootballMatchHandler>>());

        FootballSeason season = new(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            new FootballMatchRules(2, 20, 5, true, 0, false, false, 2, 5, false));
        competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);

        Result<FootballMatchDto> result = await handler.Handle(
            new CreateFootballMatchCommand(
                CompetitionId: season.Id,
                HomeTeamId: null,
                AwayTeamId: null,
                RefereeId: null,
                ScheduledDateTime: new DateTime(2027, 1, 15, 18, 0, 0, DateTimeKind.Utc),
                Venue: "Pitch"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.HomeTeamId.Should().BeNull();
        result.Data.AwayTeamId.Should().BeNull();
        matchRepo.Verify(r => r.AddAsync(It.IsAny<FootballMatch>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
