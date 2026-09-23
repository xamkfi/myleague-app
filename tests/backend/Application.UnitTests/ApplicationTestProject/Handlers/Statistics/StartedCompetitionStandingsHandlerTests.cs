using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Queries;
using FloorballStandingsHandler = Application.Features.Floorball.Statistics.Handlers.GetTeamStandingsHandler;
using FloorballSummaryHandler = Application.Features.Floorball.Statistics.Handlers.GetSeasonStatisticsSummaryHandler;
using FootballStandingsHandler = Application.Features.Football.Statistics.Handlers.GetTeamStandingsHandler;
using FootballSummaryHandler = Application.Features.Football.Statistics.Handlers.GetSeasonStatisticsSummaryHandler;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Hockey.Statistics.DTOs;
using Application.Features.Hockey.Statistics.Handlers;
using Application.Features.Hockey.Statistics.Queries;
using Application.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Statistics;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Statistics;

public class StartedCompetitionStandingsHandlerTests
{
    private static readonly DateTime Start = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_StartedFloorballSeasonWithoutStatistics_ReturnsTeamsAtZero()
    {
        FloorballSeason season = CreateFloorballSeason();
        FloorballTeam bears = CreateFloorballTeam("Bears");
        FloorballTeam wolves = CreateFloorballTeam("Wolves");
        season.AddTeam(bears);
        season.AddTeam(wolves);
        season.Activate();

        FloorballStandingsHandler handler = CreateFloorballStandingsHandler(season);

        Result<List<FloorballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFloorballTeamStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(row =>
            row.GamesPlayed == 0 && row.Points == 0 && row.GoalsFor == 0 && row.GoalsAgainst == 0);
        result.Data!.Select(row => row.TeamName).Should().Equal("Bears", "Wolves");
    }

    [Fact]
    public async Task Handle_CompletedFloorballSeasonWithoutStatistics_ReturnsTeamsAtZero()
    {
        FloorballSeason season = CreateFloorballSeason();
        season.AddTeam(CreateFloorballTeam("Bears"));
        season.Complete();

        FloorballStandingsHandler handler = CreateFloorballStandingsHandler(season);

        Result<List<FloorballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFloorballTeamStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].Points.Should().Be(0);
        result.Data![0].TeamName.Should().Be("Bears");
    }

    [Fact]
    public async Task Handle_DraftFloorballSeasonWithoutStatistics_ReturnsEmpty()
    {
        FloorballSeason season = CreateFloorballSeason();
        season.AddTeam(CreateFloorballTeam("Bears"));
        season.AddTeam(CreateFloorballTeam("Wolves"));

        FloorballStandingsHandler handler = CreateFloorballStandingsHandler(season);

        Result<List<FloorballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFloorballTeamStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_StartedFloorballTournamentWithoutStatistics_ReturnsGroupTeamsAtZero()
    {
        FloorballTournament tournament = new("Cup", Start, End);
        FloorballTeam bears = CreateFloorballTeam("Bears");
        FloorballTeam wolves = CreateFloorballTeam("Wolves");
        FloorballTournamentGroup group = tournament.AddGroup("A");
        group.AddTeam(bears);
        group.AddTeam(wolves);
        tournament.StartGroupStage();

        Mock<IFloorballTeamRepository> teams = new();
        teams.Setup(repository => repository.GetByIdAsync(bears.Id, null)).ReturnsAsync(bears);
        teams.Setup(repository => repository.GetByIdAsync(wolves.Id, null)).ReturnsAsync(wolves);

        FloorballStandingsHandler handler = CreateFloorballStandingsHandler(tournament, tournament, teams.Object);

        Result<List<FloorballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFloorballTeamStandingsQuery(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(row => row.Points == 0 && row.GamesPlayed == 0);
        result.Data!.Select(row => row.TeamId).Should().BeEquivalentTo(new[] { bears.Id, wolves.Id });
    }

    [Fact]
    public async Task Handle_DraftFloorballTournamentWithoutStatistics_ReturnsEmpty()
    {
        FloorballTournament tournament = new("Cup", Start, End);
        FloorballTournamentGroup group = tournament.AddGroup("A");
        group.AddTeam(CreateFloorballTeam("Bears"));
        group.AddTeam(CreateFloorballTeam("Wolves"));

        FloorballStandingsHandler handler = CreateFloorballStandingsHandler(tournament, tournament);

        Result<List<FloorballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFloorballTeamStandingsQuery(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_StartedFootballSeasonWithoutStatistics_ReturnsTeamsAtZero()
    {
        FootballSeason season = new("Hobby", Start, End);
        season.AddTeam(CreateFootballTeam("Bears"));
        season.AddTeam(CreateFootballTeam("Wolves"));
        season.Activate();

        FootballStandingsHandler handler = CreateFootballStandingsHandler(season);

        Result<List<FootballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFootballTeamStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Select(row => row.TeamName).Should().Equal("Bears", "Wolves");
        result.Data.Should().OnlyContain(row => row.Points == 0 && row.GamesPlayed == 0);
    }

    [Fact]
    public async Task Handle_DraftFootballTournamentWithoutStatistics_ReturnsEmpty()
    {
        FootballTournament tournament = new("Cup", Start, End);
        FootballTournamentGroup group = tournament.AddGroup("A");
        group.AddTeam(CreateFootballTeam("Bears"));
        group.AddTeam(CreateFootballTeam("Wolves"));

        FootballStandingsHandler handler = CreateFootballStandingsHandler(tournament, tournament);

        Result<List<FootballTeamSeasonStatisticsDto>> result = await handler.Handle(
            new GetFootballTeamStandingsQuery(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ActiveHockeySeasonWithoutStatistics_ReturnsTeamsAtZeroWithNames()
    {
        HockeySeason season = new("Liiga", Start, End);
        Guid bearsId = Guid.NewGuid();
        Guid wolvesId = Guid.NewGuid();
        season.AddTeam(bearsId);
        season.AddTeam(wolvesId);
        season.Publish();
        season.Activate();

        GetHockeyCompetitionStandingsHandler handler = CreateHockeyCompetitionHandler(
            season,
            new Dictionary<Guid, string>
            {
                [bearsId] = "Bears",
                [wolvesId] = "Wolves"
            });

        Result<List<HockeyTeamCompetitionStatisticsDto>> result = await handler.Handle(
            new GetHockeyCompetitionStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(row => row.Points == 0 && row.GamesPlayed == 0 && row.GoalsFor == 0);
        result.Data!.Select(row => row.TeamName).Should().Equal("Bears", "Wolves");
        result.Data!.Select(row => row.StandingRank).Should().Equal(1, 2);
    }

    [Fact]
    public async Task Handle_DraftHockeySeasonWithoutStatistics_ReturnsEmpty()
    {
        HockeySeason season = new("Liiga", Start, End);
        season.AddTeam(Guid.NewGuid());

        GetHockeyCompetitionStandingsHandler handler = CreateHockeyCompetitionHandler(season, new Dictionary<Guid, string>());

        Result<List<HockeyTeamCompetitionStatisticsDto>> result = await handler.Handle(
            new GetHockeyCompetitionStandingsQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ActiveHockeyGroupWithoutStatistics_ReturnsGroupTeamsAtZero()
    {
        HockeyTournament tournament = new("Cup", Start, End);
        Guid bearsId = Guid.NewGuid();
        Guid wolvesId = Guid.NewGuid();
        HockeyCompetitionTeam bears = tournament.AddTeam(bearsId);
        HockeyCompetitionTeam wolves = tournament.AddTeam(wolvesId);
        HockeyTournamentGroup group = tournament.AddGroup("A");
        tournament.AddTeamToGroup(group.Id, bears.Id);
        tournament.AddTeamToGroup(group.Id, wolves.Id);
        tournament.Publish();
        tournament.Activate();

        GetHockeyTournamentGroupStandingsHandler handler = CreateHockeyGroupHandler(
            tournament,
            new Dictionary<Guid, string>
            {
                [bearsId] = "Bears",
                [wolvesId] = "Wolves"
            });

        Result<List<HockeyTeamCompetitionStatisticsDto>> result = await handler.Handle(
            new GetHockeyTournamentGroupStandingsQuery(tournament.Id, group.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Select(row => row.TeamId).Should().Equal(bearsId, wolvesId);
        result.Data.Should().OnlyContain(row => row.Points == 0 && row.TournamentGroupId == group.Id);
        result.Data!.Select(row => row.TeamName).Should().Equal("Bears", "Wolves");
    }

    [Fact]
    public async Task Handle_DraftHockeyGroupWithoutStatistics_ReturnsEmpty()
    {
        HockeyTournament tournament = new("Cup", Start, End);
        HockeyCompetitionTeam bears = tournament.AddTeam(Guid.NewGuid());
        HockeyCompetitionTeam wolves = tournament.AddTeam(Guid.NewGuid());
        HockeyTournamentGroup group = tournament.AddGroup("A");
        tournament.AddTeamToGroup(group.Id, bears.Id);
        tournament.AddTeamToGroup(group.Id, wolves.Id);

        GetHockeyTournamentGroupStandingsHandler handler = CreateHockeyGroupHandler(tournament, new Dictionary<Guid, string>());

        Result<List<HockeyTeamCompetitionStatisticsDto>> result = await handler.Handle(
            new GetHockeyTournamentGroupStandingsQuery(tournament.Id, group.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NewActivatedFloorballSeason_SummaryListsEveryTeamAtZero()
    {
        FloorballSeason season = CreateFloorballSeason();
        season.AddTeam(CreateFloorballTeam("Bears"));
        season.AddTeam(CreateFloorballTeam("Wolves"));
        season.Activate();

        FloorballSummaryHandler handler = CreateFloorballSummaryHandler(season);

        Result<FloorballSeasonStatisticsSummaryDto> result = await handler.Handle(
            new GetFloorballSeasonStatisticsSummaryQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TeamStandings.Select(row => row.TeamName).Should().Equal("Bears", "Wolves");
        result.Data.TeamStandings.Should().OnlyContain(row =>
            row.GamesPlayed == 0 && row.Points == 0 && row.GoalsFor == 0 && row.GoalsAgainst == 0);
    }

    [Fact]
    public async Task Handle_NewStartedFloorballTournament_SummaryListsGroupTeamsAtZero()
    {
        FloorballTournament tournament = new("Cup", Start, End);
        FloorballTeam bears = CreateFloorballTeam("Bears");
        FloorballTeam wolves = CreateFloorballTeam("Wolves");
        FloorballTournamentGroup group = tournament.AddGroup("A");
        group.AddTeam(bears);
        group.AddTeam(wolves);
        tournament.StartGroupStage();

        Mock<IFloorballTeamRepository> teams = new();
        teams.Setup(repository => repository.GetByIdAsync(bears.Id, null)).ReturnsAsync(bears);
        teams.Setup(repository => repository.GetByIdAsync(wolves.Id, null)).ReturnsAsync(wolves);

        FloorballSummaryHandler handler = CreateFloorballSummaryHandler(tournament, tournament, teams.Object);

        Result<FloorballSeasonStatisticsSummaryDto> result = await handler.Handle(
            new GetFloorballSeasonStatisticsSummaryQuery(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TeamStandings.Should().HaveCount(2);
        result.Data.TeamStandings.Should().OnlyContain(row => row.Points == 0 && row.GamesPlayed == 0);
        result.Data.TeamStandings.Select(row => row.TeamId).Should().BeEquivalentTo(new[] { bears.Id, wolves.Id });
    }

    [Fact]
    public async Task Handle_NewDraftFloorballTournament_SummaryOmitsZeroRows()
    {
        FloorballTournament tournament = new("Cup", Start, End);
        FloorballTournamentGroup group = tournament.AddGroup("A");
        group.AddTeam(CreateFloorballTeam("Bears"));
        group.AddTeam(CreateFloorballTeam("Wolves"));

        FloorballSummaryHandler handler = CreateFloorballSummaryHandler(tournament, tournament);

        Result<FloorballSeasonStatisticsSummaryDto> result = await handler.Handle(
            new GetFloorballSeasonStatisticsSummaryQuery(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TeamStandings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NewActivatedFootballSeason_SummaryListsEveryTeamAtZero()
    {
        FootballSeason season = new("Hobby", Start, End);
        season.AddTeam(CreateFootballTeam("Bears"));
        season.AddTeam(CreateFootballTeam("Wolves"));
        season.Activate();

        FootballSummaryHandler handler = CreateFootballSummaryHandler(season);

        Result<FootballSeasonStatisticsSummaryDto> result = await handler.Handle(
            new GetFootballSeasonStatisticsSummaryQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TeamStandings.Select(row => row.TeamName).Should().Equal("Bears", "Wolves");
        result.Data.TeamStandings.Should().OnlyContain(row => row.Points == 0 && row.GamesPlayed == 0);
    }

    [Fact]
    public async Task Handle_NewStartedFootballTournament_SummaryListsGroupTeamsAtZero()
    {
        FootballTournament tournament = new("Cup", Start, End);
        FootballTeam bears = CreateFootballTeam("Bears");
        FootballTeam wolves = CreateFootballTeam("Wolves");
        FootballTournamentGroup group = tournament.AddGroup("A");
        group.AddTeam(bears);
        group.AddTeam(wolves);
        tournament.StartGroupStage();

        Mock<IFootballTeamRepository> teams = new();
        teams.Setup(repository => repository.GetByIdAsync(bears.Id)).ReturnsAsync(bears);
        teams.Setup(repository => repository.GetByIdAsync(wolves.Id)).ReturnsAsync(wolves);

        FootballSummaryHandler handler = CreateFootballSummaryHandler(tournament, tournament, teams.Object);

        Result<FootballSeasonStatisticsSummaryDto> result = await handler.Handle(
            new GetFootballSeasonStatisticsSummaryQuery(tournament.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TeamStandings.Select(row => row.TeamId).Should().BeEquivalentTo(new[] { bears.Id, wolves.Id });
        result.Data.TeamStandings.Should().OnlyContain(row => row.Points == 0 && row.GamesPlayed == 0);
    }

    private static FloorballSeason CreateFloorballSeason() => new("2026-2027", Start, End);

    private static FloorballTeam CreateFloorballTeam(string name)
    {
        Club club = new(name + " HC");
        return new FloorballTeam(name, null, club, "Arena", "Blue", TeamCategory.Adult);
    }

    private static FootballTeam CreateFootballTeam(string name)
    {
        Club club = new(name + " FC");
        return new FootballTeam(name, null, club, "Pitch", "Red", TeamCategory.Adult);
    }

    private static FloorballStandingsHandler CreateFloorballStandingsHandler(
        FloorballCompetition competition,
        FloorballTournament? withGroups = null,
        IFloorballTeamRepository? teams = null)
    {
        Mock<IFloorballStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamStandingsAsync(competition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FloorballTeamSeasonStatistics>());

        Mock<IFloorballCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(competition.Id)).ReturnsAsync(competition);

        Mock<IFloorballTournamentRepository> tournaments = new();
        tournaments
            .Setup(repository => repository.GetByIdWithGroupsAsNoTrackingAsync(competition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withGroups);

        return new FloorballStandingsHandler(
            statistics.Object,
            competitions.Object,
            tournaments.Object,
            teams ?? new Mock<IFloorballTeamRepository>().Object,
            Mock.Of<ILogger<FloorballStandingsHandler>>());
    }

    private static FootballStandingsHandler CreateFootballStandingsHandler(
        FootballCompetition competition,
        FootballTournament? withGroups = null)
    {
        Mock<IFootballStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamStandingsAsync(competition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Football.Statistics.FootballTeamSeasonStatistics>());

        Mock<IFootballCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(competition.Id)).ReturnsAsync(competition);

        Mock<IFootballTournamentRepository> tournaments = new();
        tournaments
            .Setup(repository => repository.GetByIdWithGroupsAsNoTrackingAsync(competition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withGroups);

        return new FootballStandingsHandler(
            statistics.Object,
            competitions.Object,
            tournaments.Object,
            new Mock<IFootballTeamRepository>().Object,
            Mock.Of<ILogger<FootballStandingsHandler>>());
    }

    private static FloorballSummaryHandler CreateFloorballSummaryHandler(
        FloorballCompetition competition,
        FloorballTournament? withGroups = null,
        IFloorballTeamRepository? teams = null)
    {
        Mock<IFloorballStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamStandingsAsync(competition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FloorballTeamSeasonStatistics>());
        statistics
            .Setup(repository => repository.GetTopScorersAsync(competition.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FloorballPlayerSeasonStatistics>());
        statistics
            .Setup(repository => repository.GetTopAssistsAsync(competition.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FloorballPlayerSeasonStatistics>());
        statistics
            .Setup(repository => repository.GetTopGoaliesAsync(competition.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FloorballGoalieSeasonStatistics>());

        Mock<IFloorballCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(competition.Id)).ReturnsAsync(competition);

        Mock<IFloorballTournamentRepository> tournaments = new();
        tournaments
            .Setup(repository => repository.GetByIdWithGroupsAsNoTrackingAsync(competition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withGroups);

        return new FloorballSummaryHandler(
            statistics.Object,
            new Mock<IFloorballPlayerRepository>().Object,
            teams ?? new Mock<IFloorballTeamRepository>().Object,
            new Mock<IFloorballMatchRepository>().Object,
            competitions.Object,
            tournaments.Object,
            new Mock<IPersonRepository>().Object,
            Mock.Of<ILogger<FloorballSummaryHandler>>());
    }

    private static FootballSummaryHandler CreateFootballSummaryHandler(
        FootballCompetition competition,
        FootballTournament? withGroups = null,
        IFootballTeamRepository? teams = null)
    {
        Mock<IFootballStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamStandingsAsync(competition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Football.Statistics.FootballTeamSeasonStatistics>());
        statistics
            .Setup(repository => repository.GetTopScorersAsync(competition.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Football.Statistics.FootballPlayerSeasonStatistics>());
        statistics
            .Setup(repository => repository.GetTopAssistsAsync(competition.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Football.Statistics.FootballPlayerSeasonStatistics>());

        Mock<IFootballCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(competition.Id)).ReturnsAsync(competition);

        Mock<IFootballTournamentRepository> tournaments = new();
        tournaments
            .Setup(repository => repository.GetByIdWithGroupsAsNoTrackingAsync(competition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withGroups);

        return new FootballSummaryHandler(
            statistics.Object,
            new Mock<IFootballPlayerRepository>().Object,
            new Mock<IFootballMatchRepository>().Object,
            competitions.Object,
            tournaments.Object,
            teams ?? new Mock<IFootballTeamRepository>().Object,
            new Mock<IPersonRepository>().Object,
            Mock.Of<ILogger<FootballSummaryHandler>>());
    }

    private static GetHockeyCompetitionStandingsHandler CreateHockeyCompetitionHandler(
        HockeyCompetition competition,
        IReadOnlyDictionary<Guid, string> names)
    {
        Mock<IHockeyStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamCompetitionStatisticsAsync(
                competition.Id,
                HockeyStatisticsScope.Competition,
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(new List<HockeyTeamCompetitionStatistics>());

        Mock<IHockeyCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(competition.Id)).ReturnsAsync(competition);

        Mock<IHockeyTeamRepository> teams = new();
        teams
            .Setup(repository => repository.GetNamesByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(names);

        return new GetHockeyCompetitionStandingsHandler(
            statistics.Object,
            competitions.Object,
            teams.Object,
            Mock.Of<ILogger<GetHockeyCompetitionStandingsHandler>>());
    }

    private static GetHockeyTournamentGroupStandingsHandler CreateHockeyGroupHandler(
        HockeyTournament tournament,
        IReadOnlyDictionary<Guid, string> names)
    {
        Mock<IHockeyStatisticsRepository> statistics = new();
        statistics
            .Setup(repository => repository.GetTeamCompetitionStatisticsAsync(
                tournament.Id,
                HockeyStatisticsScope.TournamentGroup,
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(new List<HockeyTeamCompetitionStatistics>());

        Mock<IHockeyCompetitionRepository> competitions = new();
        competitions.Setup(repository => repository.GetByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        Mock<IHockeyTeamRepository> teams = new();
        teams
            .Setup(repository => repository.GetNamesByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(names);

        return new GetHockeyTournamentGroupStandingsHandler(
            statistics.Object,
            competitions.Object,
            teams.Object,
            Mock.Of<ILogger<GetHockeyTournamentGroupStandingsHandler>>());
    }
}
