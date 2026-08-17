using Application.Common;
using Application.Features.Hockey.Statistics.Commands;
using Application.Features.Hockey.Statistics.DTOs;
using Application.Features.Hockey.Statistics.Handlers;
using Application.Features.Hockey.Statistics.Queries;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Statistics;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class HockeyStatisticsHandlerTests
{
    private readonly Mock<IHockeyMatchRepository> _matchRepo = new();
    private readonly Mock<IHockeyTeamRepository> _teamRepo = new();
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyStatisticsRepository> _statsRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    private static HockeyMatch CreateStandaloneMatch() =>
        new(
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.Friendly,
            venue: "Nokia Arena");

    private static HockeySeason CreateSeason() =>
        new(
            "Test Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task RecalculateMatch_MissingMatch_ReturnsNotFound()
    {
        Guid missingId = Guid.NewGuid();
        _matchRepo.Setup(r => r.GetByIdForStatisticsAsync(missingId)).ReturnsAsync((HockeyMatch?)null);

        RecalculateHockeyMatchStatisticsHandler handler = new(
            _matchRepo.Object,
            _teamRepo.Object,
            _statsRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RecalculateHockeyMatchStatisticsHandler>>());

        Result result = await handler.Handle(
            new RecalculateHockeyMatchStatisticsCommand(missingId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _statsRepo.Verify(
            r => r.ReplaceMatchStatisticsAsync(
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyList<HockeyMatchTeamStatistics>>(),
                It.IsAny<IReadOnlyList<HockeyMatchPlayerStatistics>>(),
                It.IsAny<IReadOnlyList<HockeyGoalieMatchStatistics>>()),
            Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecalculateMatch_ExistingMatch_ReplacesAndSaves()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);
        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away);

        _matchRepo.Setup(r => r.GetByIdForStatisticsAsync(match.Id)).ReturnsAsync(match);
        _teamRepo.Setup(r => r.GetByIdAsync(home.Id)).ReturnsAsync(home);
        _teamRepo.Setup(r => r.GetByIdAsync(away.Id)).ReturnsAsync(away);

        RecalculateHockeyMatchStatisticsHandler handler = new(
            _matchRepo.Object,
            _teamRepo.Object,
            _statsRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RecalculateHockeyMatchStatisticsHandler>>());

        Result result = await handler.Handle(
            new RecalculateHockeyMatchStatisticsCommand(match.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _statsRepo.Verify(
            r => r.ReplaceMatchStatisticsAsync(
                match.Id,
                It.Is<IReadOnlyList<HockeyMatchTeamStatistics>>(t => t.Count == 2),
                It.IsAny<IReadOnlyList<HockeyMatchPlayerStatistics>>(),
                It.IsAny<IReadOnlyList<HockeyGoalieMatchStatistics>>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecalculateCompetition_MissingCompetition_ReturnsNotFound()
    {
        Guid missingId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((HockeyCompetition?)null);

        RecalculateHockeyCompetitionStatisticsHandler handler = new(
            _competitionRepo.Object,
            _matchRepo.Object,
            _teamRepo.Object,
            _statsRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RecalculateHockeyCompetitionStatisticsHandler>>());

        Result result = await handler.Handle(
            new RecalculateHockeyCompetitionStatisticsCommand(missingId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecalculateCompetition_WithFinishedMatch_SetsStandingRanks()
    {
        HockeySeason season = CreateSeason();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyTeam away = new("Ilves", club, TeamCategory.Adult);
        HockeyCompetitionTeam homeCt = season.AddTeam(home.Id, seed: 1);
        HockeyCompetitionTeam awayCt = season.AddTeam(away.Id, seed: 2);

        HockeyMatch match = new(
            new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.League,
            competitionId: season.Id);

        match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home, homeCt);
        match.AssignMatchTeam(away.Id, HockeyTeamSlot.Away, awayCt);
        match.MarkFinished(resultType: HockeyMatchResultType.HomeWin);

        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _matchRepo.Setup(r => r.GetByCompetitionIdForStatisticsAsync(season.Id))
            .ReturnsAsync(new List<HockeyMatch> { match });
        _teamRepo.Setup(r => r.GetByIdAsync(home.Id)).ReturnsAsync(home);
        _teamRepo.Setup(r => r.GetByIdAsync(away.Id)).ReturnsAsync(away);

        List<HockeyTeamCompetitionStatistics>? capturedTeams = null;
        _statsRepo
            .Setup(r => r.ReplaceCompetitionStatisticsAsync(
                season.Id,
                HockeyStatisticsScope.Competition,
                null,
                null,
                null,
                It.IsAny<IReadOnlyList<HockeyTeamCompetitionStatistics>>(),
                It.IsAny<IReadOnlyList<HockeyPlayerCompetitionStatistics>>(),
                It.IsAny<IReadOnlyList<HockeyGoalieCompetitionStatistics>>()))
            .Callback<Guid, HockeyStatisticsScope, Guid?, Guid?, Guid?,
                IReadOnlyList<HockeyTeamCompetitionStatistics>,
                IReadOnlyList<HockeyPlayerCompetitionStatistics>,
                IReadOnlyList<HockeyGoalieCompetitionStatistics>>(
                (_, _, _, _, _, teams, _, _) => capturedTeams = teams.ToList())
            .Returns(Task.CompletedTask);

        RecalculateHockeyCompetitionStatisticsHandler handler = new(
            _competitionRepo.Object,
            _matchRepo.Object,
            _teamRepo.Object,
            _statsRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RecalculateHockeyCompetitionStatisticsHandler>>());

        Result result = await handler.Handle(
            new RecalculateHockeyCompetitionStatisticsCommand(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedTeams.Should().NotBeNull();
        capturedTeams!.Should().HaveCount(2);
        capturedTeams.Select(t => t.StandingRank).Should().BeEquivalentTo(new[] { 1, 2 });
        capturedTeams.Single(t => t.TeamId == home.Id).StandingRank.Should().Be(1);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMatchStatistics_Existing_ReturnsMappedDto()
    {
        HockeyMatch match = CreateStandaloneMatch();
        Club club = new("Tappara HC");
        HockeyTeam home = new("Tappara", club, TeamCategory.Adult);
        HockeyMatchTeam matchTeam = match.AssignMatchTeam(home.Id, HockeyTeamSlot.Home);

        HockeyMatchTeamStatistics teamStats = new(match.Id, matchTeam.Id, home.Id);
        _matchRepo.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _statsRepo.Setup(r => r.GetMatchTeamStatisticsAsync(match.Id))
            .ReturnsAsync(new List<HockeyMatchTeamStatistics> { teamStats });
        _statsRepo.Setup(r => r.GetMatchPlayerStatisticsAsync(match.Id))
            .ReturnsAsync(Array.Empty<HockeyMatchPlayerStatistics>());
        _statsRepo.Setup(r => r.GetGoalieMatchStatisticsAsync(match.Id))
            .ReturnsAsync(Array.Empty<HockeyGoalieMatchStatistics>());

        GetHockeyMatchStatisticsHandler handler = new(
            _matchRepo.Object,
            _statsRepo.Object,
            Mock.Of<ILogger<GetHockeyMatchStatisticsHandler>>());

        Result<HockeyMatchStatisticsDto> result = await handler.Handle(
            new GetHockeyMatchStatisticsQuery(match.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.MatchId.Should().Be(match.Id);
        result.Data.Teams.Should().HaveCount(1);
        result.Data.Teams[0].TeamId.Should().Be(home.Id);
    }

    [Fact]
    public async Task GetCompetitionStandings_ReturnsMappedDtos()
    {
        Guid competitionId = Guid.NewGuid();
        Guid teamId = Guid.NewGuid();
        HockeyTeamCompetitionStatistics standing = new(
            teamId,
            competitionId,
            HockeyStatisticsScope.Competition);
        standing.SetStandingRank(1);

        _statsRepo.Setup(r => r.GetTeamCompetitionStatisticsAsync(
                competitionId,
                HockeyStatisticsScope.Competition,
                null,
                null,
                null))
            .ReturnsAsync(new List<HockeyTeamCompetitionStatistics> { standing });

        GetHockeyCompetitionStandingsHandler handler = new(
            _statsRepo.Object,
            Mock.Of<ILogger<GetHockeyCompetitionStandingsHandler>>());

        Result<List<HockeyTeamCompetitionStatisticsDto>> result = await handler.Handle(
            new GetHockeyCompetitionStandingsQuery(competitionId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].TeamId.Should().Be(teamId);
        result.Data[0].StandingRank.Should().Be(1);
    }
}
