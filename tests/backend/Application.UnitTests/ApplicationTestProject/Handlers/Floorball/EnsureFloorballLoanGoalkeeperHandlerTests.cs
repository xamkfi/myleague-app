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

public class EnsureFloorballLoanGoalkeeperHandlerTests
{
    private readonly Mock<IFloorballTeamRepository> _teams = new();
    private readonly Mock<IFloorballPlayerRepository> _players = new();
    private readonly Mock<IPersonRepository> _persons = new();
    private readonly Mock<IUnitOfWork> _commonUnitOfWork = new();
    private readonly Mock<IFloorballUnitOfWork> _floorballUnitOfWork = new();
    private FloorballPlayer? _createdPlayer;

    public EnsureFloorballLoanGoalkeeperHandlerTests()
    {
        _persons.Setup(r => r.AddAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
        _commonUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _floorballUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _players.Setup(r => r.AddAsync(It.IsAny<FloorballPlayer>()))
            .Callback<FloorballPlayer>(player => _createdPlayer = player)
            .Returns(Task.CompletedTask);
        _players.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<Guid> ids, CancellationToken _) =>
            {
                Dictionary<Guid, FloorballPlayer> found = new();
                if (_createdPlayer != null && ids.Contains(_createdPlayer.Id))
                {
                    found[_createdPlayer.Id] = _createdPlayer;
                }
                return Task.FromResult(found);
            });
    }

    [Fact]
    public async Task Handle_TeamMissing_ReturnsNotFound()
    {
        Guid teamId = Guid.NewGuid();
        _teams.Setup(r => r.GetByIdAsync(teamId, It.IsAny<Guid?>())).ReturnsAsync((FloorballTeam?)null);

        Result<FloorballLoanGoalkeeperDto> result = await CreateHandler().Handle(
            new EnsureFloorballLoanGoalkeeperCommand(teamId, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _persons.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CalledTwice_ReusesTheSamePlayer()
    {
        FloorballTeam team = CreateTeam();
        _teams.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<Guid?>())).ReturnsAsync(team);
        EnsureFloorballLoanGoalkeeperCommand command = new(team.Id, null);
        EnsureFloorballLoanGoalkeeperHandler handler = CreateHandler();

        Result<FloorballLoanGoalkeeperDto> first = await handler.Handle(command, CancellationToken.None);
        Result<FloorballLoanGoalkeeperDto> second = await handler.Handle(command, CancellationToken.None);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Data!.PlayerId.Should().Be(first.Data!.PlayerId);
        second.Data.DisplayName.Should().Be(LoanGoalkeeperNames.DisplayName);
        second.Data.IsLoanGoalkeeper.Should().BeTrue();
        team.Roster.Should().ContainSingle(row => row.PlayerId == first.Data.PlayerId && row.JerseyNumber == null);
        _persons.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Once);
        _players.Verify(r => r.AddAsync(It.IsAny<FloorballPlayer>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingLoanGoalkeeperOnAnotherRoster_AddsCompetitionMembership()
    {
        FloorballTeam team = CreateTeam();
        FloorballPlayer loanGoalkeeper = new(Guid.NewGuid(), new Position(FloorballPosition.Goalkeeper));
        loanGoalkeeper.MarkAsLoanGoalkeeper();
        team.AddPlayer(loanGoalkeeper, FloorballPosition.Goalkeeper);
        Guid competitionId = Guid.NewGuid();
        _createdPlayer = loanGoalkeeper;
        _teams.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<Guid?>())).ReturnsAsync(team);

        Result<FloorballLoanGoalkeeperDto> result = await CreateHandler().Handle(
            new EnsureFloorballLoanGoalkeeperCommand(team.Id, competitionId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PlayerId.Should().Be(loanGoalkeeper.Id);
        team.Roster.Should().Contain(row => row.PlayerId == loanGoalkeeper.Id && row.CompetitionId == competitionId);
        team.Roster.Should().Contain(row => row.PlayerId == loanGoalkeeper.Id && row.CompetitionId == null);
        _players.Verify(r => r.AddAsync(It.IsAny<FloorballPlayer>()), Times.Never);
    }

    private EnsureFloorballLoanGoalkeeperHandler CreateHandler() => new(
        _teams.Object,
        _players.Object,
        _persons.Object,
        _commonUnitOfWork.Object,
        _floorballUnitOfWork.Object,
        Mock.Of<ILogger<EnsureFloorballLoanGoalkeeperHandler>>());

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
