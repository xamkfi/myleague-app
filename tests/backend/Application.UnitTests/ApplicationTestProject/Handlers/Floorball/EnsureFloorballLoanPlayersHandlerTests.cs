using Application.Common;
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Teams.Handlers;
using Domain.Constants;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class EnsureFloorballLoanPlayersHandlerTests
{
    private readonly Mock<IFloorballTeamRepository> _teams = new();
    private readonly Mock<IFloorballPlayerRepository> _players = new();
    private readonly Mock<IPersonRepository> _persons = new();
    private readonly Mock<IUnitOfWork> _commonUnitOfWork = new();
    private readonly Mock<IFloorballUnitOfWork> _floorballUnitOfWork = new();
    private readonly List<FloorballPlayer> _knownPlayers = new();

    public EnsureFloorballLoanPlayersHandlerTests()
    {
        _persons.Setup(r => r.AddAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
        _commonUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _floorballUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _players.Setup(r => r.AddAsync(It.IsAny<FloorballPlayer>()))
            .Callback<FloorballPlayer>(player => _knownPlayers.Add(player))
            .Returns(Task.CompletedTask);
        _players.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<Guid> ids, CancellationToken _) =>
            {
                Dictionary<Guid, FloorballPlayer> found = new();
                foreach (Guid id in ids)
                {
                    FloorballPlayer? player = _knownPlayers.FirstOrDefault(item => item.Id == id);
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
        FloorballTeam team = CreateTeam();
        _teams.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<Guid?>())).ReturnsAsync(team);
        EnsureFloorballLoanPlayersHandler handler = CreateHandler();

        Result<IReadOnlyList<FloorballLoanPlayerDto>> result = await handler.Handle(
            new EnsureFloorballLoanPlayersCommand(team.Id, null, 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].DisplayName.Should().Be(LoanPlayerNames.DisplayName(1));
        result.Data[0].LoanPlayerNumber.Should().Be(1);
        result.Data[0].Position.Should().Be(FloorballPosition.Forward);
        result.Data[1].DisplayName.Should().Be(LoanPlayerNames.DisplayName(2));
        result.Data[1].JerseyNumber.Should().NotBe(result.Data[0].JerseyNumber);
        result.Data.Select(player => player.JerseyNumber).Should().OnlyContain(number => number >= 1 && number <= 99);
        _persons.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_CalledTwiceWithSameCount_DoesNotCreateMorePlayers()
    {
        FloorballTeam team = CreateTeam();
        _teams.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<Guid?>())).ReturnsAsync(team);
        EnsureFloorballLoanPlayersCommand command = new(team.Id, Guid.NewGuid(), 2);
        EnsureFloorballLoanPlayersHandler handler = CreateHandler();

        Result<IReadOnlyList<FloorballLoanPlayerDto>> first = await handler.Handle(command, CancellationToken.None);
        Result<IReadOnlyList<FloorballLoanPlayerDto>> second = await handler.Handle(command, CancellationToken.None);

        second.IsSuccess.Should().BeTrue();
        second.Data!.Select(player => player.PlayerId).Should().Equal(first.Data!.Select(player => player.PlayerId));
        _players.Verify(r => r.AddAsync(It.IsAny<FloorballPlayer>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_OneExistingAndCountTwo_CreatesOnlyTheMissingPlayer()
    {
        FloorballTeam team = CreateTeam();
        FloorballPlayer existing = new(Guid.NewGuid(), new Position(FloorballPosition.Forward));
        existing.MarkAsLoanPlayer(1);
        team.AddPlayer(existing, FloorballPosition.Forward, jerseyNumber: 4);
        _knownPlayers.Add(existing);
        _teams.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<Guid?>())).ReturnsAsync(team);

        Result<IReadOnlyList<FloorballLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureFloorballLoanPlayersCommand(team.Id, null, 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].PlayerId.Should().Be(existing.Id);
        result.Data[0].DisplayName.Should().Be(LoanPlayerNames.DisplayName(1));
        result.Data[0].JerseyNumber.Should().Be(4);
        result.Data[1].LoanPlayerNumber.Should().Be(2);
        result.Data[1].DisplayName.Should().Be(LoanPlayerNames.DisplayName(2));
        _players.Verify(r => r.AddAsync(It.IsAny<FloorballPlayer>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TakenJersey_AssignsTheNextFreeNumber()
    {
        FloorballTeam team = CreateTeam();
        FloorballPlayer regular = new(Guid.NewGuid(), new Position(FloorballPosition.Defender));
        team.AddPlayer(regular, FloorballPosition.Defender, jerseyNumber: 1);
        _teams.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<Guid?>())).ReturnsAsync(team);

        Result<IReadOnlyList<FloorballLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureFloorballLoanPlayersCommand(team.Id, null, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].JerseyNumber.Should().Be(2);
    }

    [Fact]
    public async Task Handle_InactiveLoanPlayer_ReactivatesAndAddsCompetitionMembership()
    {
        FloorballTeam team = CreateTeam();
        FloorballPlayer existing = new(Guid.NewGuid(), new Position(FloorballPosition.Forward));
        existing.MarkAsLoanPlayer(1);
        existing.UpdateActiveStatus(false);
        team.AddPlayer(existing, FloorballPosition.Forward, jerseyNumber: 9);
        _knownPlayers.Add(existing);
        Guid competitionId = Guid.NewGuid();
        _teams.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<Guid?>())).ReturnsAsync(team);

        Result<IReadOnlyList<FloorballLoanPlayerDto>> result = await CreateHandler().Handle(
            new EnsureFloorballLoanPlayersCommand(team.Id, competitionId, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].PlayerId.Should().Be(existing.Id);
        result.Data[0].JerseyNumber.Should().Be(9);
        existing.IsActive.Should().BeTrue();
        team.Roster.Should().Contain(row => row.PlayerId == existing.Id && row.CompetitionId == competitionId);
        _players.Verify(r => r.AddAsync(It.IsAny<FloorballPlayer>()), Times.Never);
    }

    private EnsureFloorballLoanPlayersHandler CreateHandler() => new(
        _teams.Object,
        _players.Object,
        _persons.Object,
        _commonUnitOfWork.Object,
        _floorballUnitOfWork.Object,
        Mock.Of<ILogger<EnsureFloorballLoanPlayersHandler>>());

    private static FloorballTeam CreateTeam()
    {
        Club club = new("Test Club");
        return new FloorballTeam(
            "Wolves",
            divisionId: null,
            club,
            homeArena: "Arena",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }
}
