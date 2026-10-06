using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Handlers;
using Domain.Constants;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class EnsureHockeyLoanPlayersHandlerTests
{
    private readonly Mock<IHockeyTeamRepository> _teams = new();
    private readonly Mock<IHockeyPlayerRepository> _players = new();
    private readonly Mock<IPersonRepository> _persons = new();
    private readonly Mock<IUnitOfWork> _commonUnitOfWork = new();
    private readonly Mock<IHockeyUnitOfWork> _hockeyUnitOfWork = new();
    private readonly List<HockeyPlayer> _knownPlayers = new();

    public EnsureHockeyLoanPlayersHandlerTests()
    {
        _persons.Setup(r => r.AddAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
        _commonUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _hockeyUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _players.Setup(r => r.AddAsync(It.IsAny<HockeyPlayer>()))
            .Callback<HockeyPlayer>(player => _knownPlayers.Add(player))
            .Returns(Task.CompletedTask);
        _players.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<Guid> ids, CancellationToken _) =>
            {
                Dictionary<Guid, HockeyPlayer> found = new();
                foreach (Guid id in ids)
                {
                    HockeyPlayer? player = _knownPlayers.FirstOrDefault(item => item.Id == id);
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
        HockeyTeam team = CreateTeam();
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        Result<IReadOnlyList<HockeyLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureHockeyLoanPlayersCommand(team.Id, Guid.NewGuid(), 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].DisplayName.Should().Be(LoanPlayerNames.DisplayName(1));
        result.Data[0].Position.Should().Be(HockeyPosition.Center);
        result.Data[1].DisplayName.Should().Be(LoanPlayerNames.DisplayName(2));
        result.Data[1].JerseyNumber.Should().NotBe(result.Data[0].JerseyNumber);
        _persons.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_CalledTwiceWithSameCount_DoesNotCreateMorePlayers()
    {
        HockeyTeam team = CreateTeam();
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        EnsureHockeyLoanPlayersCommand command = new(team.Id, Guid.NewGuid(), 2);
        EnsureHockeyLoanPlayersHandler handler = CreateHandler();

        Result<IReadOnlyList<HockeyLoanPlayerDto>> first = await handler.Handle(command, CancellationToken.None);
        Result<IReadOnlyList<HockeyLoanPlayerDto>> second = await handler.Handle(command, CancellationToken.None);

        second.IsSuccess.Should().BeTrue();
        second.Data!.Select(player => player.PlayerId).Should().Equal(first.Data!.Select(player => player.PlayerId));
        _players.Verify(r => r.AddAsync(It.IsAny<HockeyPlayer>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_OneExistingAndCountTwo_CreatesOnlyTheMissingPlayer()
    {
        HockeyTeam team = CreateTeam();
        HockeyPlayer existing = new(Guid.NewGuid(), HockeyPosition.Center);
        existing.MarkAsLoanPlayer(1);
        team.AddPlayer(existing, HockeyPosition.Center, jerseyNumber: 4);
        _knownPlayers.Add(existing);
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        Result<IReadOnlyList<HockeyLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureHockeyLoanPlayersCommand(team.Id, null, 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].PlayerId.Should().Be(existing.Id);
        result.Data[0].JerseyNumber.Should().Be(4);
        result.Data[1].DisplayName.Should().Be(LoanPlayerNames.DisplayName(2));
        _players.Verify(r => r.AddAsync(It.IsAny<HockeyPlayer>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TakenJersey_AssignsTheNextFreeNumber()
    {
        HockeyTeam team = CreateTeam();
        HockeyPlayer regular = new(Guid.NewGuid(), HockeyPosition.Defenseman);
        team.AddPlayer(regular, HockeyPosition.Defenseman, jerseyNumber: 1);
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        Result<IReadOnlyList<HockeyLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureHockeyLoanPlayersCommand(team.Id, null, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].JerseyNumber.Should().Be(2);
    }

    [Fact]
    public async Task Handle_InactiveLoanPlayer_ReactivatesAndAddsCompetitionMembership()
    {
        HockeyTeam team = CreateTeam();
        HockeyPlayer existing = new(Guid.NewGuid(), HockeyPosition.Center);
        existing.MarkAsLoanPlayer(1);
        existing.UpdateActiveStatus(false);
        team.AddPlayer(existing, HockeyPosition.Center, jerseyNumber: 9);
        _knownPlayers.Add(existing);
        Guid competitionId = Guid.NewGuid();
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);

        Result<IReadOnlyList<HockeyLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureHockeyLoanPlayersCommand(team.Id, competitionId, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].PlayerId.Should().Be(existing.Id);
        result.Data[0].JerseyNumber.Should().Be(9);
        existing.IsActive.Should().BeTrue();
        team.Roster.Should().Contain(row => row.PlayerId == existing.Id && row.CompetitionId == competitionId && row.IsActive);
        _players.Verify(r => r.AddAsync(It.IsAny<HockeyPlayer>()), Times.Never);
    }

    private EnsureHockeyLoanPlayersHandler CreateHandler() => new(
        _teams.Object,
        _players.Object,
        _persons.Object,
        _commonUnitOfWork.Object,
        _hockeyUnitOfWork.Object,
        Mock.Of<ILogger<EnsureHockeyLoanPlayersHandler>>());

    private static HockeyTeam CreateTeam()
    {
        Club club = new("Test Club");
        return new HockeyTeam("Wolves", club, TeamCategory.Adult, shortName: "WOL");
    }
}
