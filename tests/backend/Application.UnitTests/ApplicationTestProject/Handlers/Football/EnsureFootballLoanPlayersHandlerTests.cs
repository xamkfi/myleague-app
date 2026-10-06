using Application.Common;
using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Teams.Handlers;
using Domain.Constants;
using Domain.Entities.Common;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Football;

public class EnsureFootballLoanPlayersHandlerTests
{
    private readonly Mock<IFootballTeamRepository> _teams = new();
    private readonly Mock<IFootballPlayerRepository> _players = new();
    private readonly Mock<IPersonRepository> _persons = new();
    private readonly Mock<IUnitOfWork> _commonUnitOfWork = new();
    private readonly Mock<IFootballUnitOfWork> _footballUnitOfWork = new();
    private readonly List<FootballPlayer> _knownPlayers = new();

    public EnsureFootballLoanPlayersHandlerTests()
    {
        _persons.Setup(r => r.AddAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
        _commonUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _footballUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _players.Setup(r => r.AddAsync(It.IsAny<FootballPlayer>()))
            .Callback<FootballPlayer>(player => _knownPlayers.Add(player))
            .Returns(Task.CompletedTask);
        _players.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<Guid> ids, CancellationToken _) =>
            {
                Dictionary<Guid, FootballPlayer> found = new();
                foreach (Guid id in ids)
                {
                    FootballPlayer? player = _knownPlayers.FirstOrDefault(item => item.Id == id);
                    if (player != null)
                    {
                        found[id] = player;
                    }
                }

                return Task.FromResult(found);
            });
    }

    [Fact]
    public async Task Handle_CountTwoFromEmpty_CreatesTwoNamedPlayersWithDistinctNumbers()
    {
        FootballTeam team = CreateTeam();
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        Result<IReadOnlyList<FootballLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureFootballLoanPlayersCommand(team.Id, Guid.NewGuid(), 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].DisplayName.Should().Be(LoanPlayerNames.DisplayName(1));
        result.Data[0].Position.Should().Be(FootballPosition.Midfielder);
        result.Data[1].DisplayName.Should().Be(LoanPlayerNames.DisplayName(2));
        result.Data[1].JerseyNumber.Should().NotBe(result.Data[0].JerseyNumber);
        _persons.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_CalledTwiceWithSameCount_DoesNotCreateMorePlayers()
    {
        FootballTeam team = CreateTeam();
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        EnsureFootballLoanPlayersCommand command = new(team.Id, Guid.NewGuid(), 2);
        EnsureFootballLoanPlayersHandler handler = CreateHandler();

        Result<IReadOnlyList<FootballLoanPlayerDto>> first = await handler.Handle(command, CancellationToken.None);
        Result<IReadOnlyList<FootballLoanPlayerDto>> second = await handler.Handle(command, CancellationToken.None);

        second.IsSuccess.Should().BeTrue();
        second.Data!.Select(player => player.PlayerId).Should().Equal(first.Data!.Select(player => player.PlayerId));
        _players.Verify(r => r.AddAsync(It.IsAny<FootballPlayer>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_OneExistingAndCountTwo_CreatesOnlyTheMissingPlayer()
    {
        FootballTeam team = CreateTeam();
        FootballPlayer existing = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));
        existing.MarkAsLoanPlayer(1);
        team.AddPlayer(existing, FootballPosition.Midfielder, jerseyNumber: 4);
        _knownPlayers.Add(existing);
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        Result<IReadOnlyList<FootballLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureFootballLoanPlayersCommand(team.Id, null, 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].PlayerId.Should().Be(existing.Id);
        result.Data[0].JerseyNumber.Should().Be(4);
        result.Data[1].DisplayName.Should().Be(LoanPlayerNames.DisplayName(2));
        _players.Verify(r => r.AddAsync(It.IsAny<FootballPlayer>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TakenJersey_AssignsTheNextFreeNumber()
    {
        FootballTeam team = CreateTeam();
        FootballPlayer regular = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Defender));
        team.AddPlayer(regular, FootballPosition.Defender, jerseyNumber: 1);
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        Result<IReadOnlyList<FootballLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureFootballLoanPlayersCommand(team.Id, null, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].JerseyNumber.Should().Be(2);
    }

    [Fact]
    public async Task Handle_InactiveLoanPlayer_ReactivatesAndAddsCompetitionMembership()
    {
        FootballTeam team = CreateTeam();
        FootballPlayer existing = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));
        existing.MarkAsLoanPlayer(1);
        existing.UpdateActiveStatus(false);
        team.AddPlayer(existing, FootballPosition.Midfielder, jerseyNumber: 9);
        _knownPlayers.Add(existing);
        Guid competitionId = Guid.NewGuid();
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        Result<IReadOnlyList<FootballLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureFootballLoanPlayersCommand(team.Id, competitionId, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].PlayerId.Should().Be(existing.Id);
        result.Data[0].JerseyNumber.Should().Be(9);
        existing.IsActive.Should().BeTrue();
        team.Roster.Should().Contain(row => row.PlayerId == existing.Id && row.CompetitionId == competitionId);
        _players.Verify(r => r.AddAsync(It.IsAny<FootballPlayer>()), Times.Never);
    }

    private EnsureFootballLoanPlayersHandler CreateHandler() => new(
        _teams.Object,
        _players.Object,
        _persons.Object,
        _commonUnitOfWork.Object,
        _footballUnitOfWork.Object,
        Mock.Of<ILogger<EnsureFootballLoanPlayersHandler>>());

    private static FootballTeam CreateTeam()
    {
        Club club = new("Test Club");
        return new FootballTeam(
            "Wolves",
            divisionId: null,
            club,
            homeArena: "Pitch",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }
}
