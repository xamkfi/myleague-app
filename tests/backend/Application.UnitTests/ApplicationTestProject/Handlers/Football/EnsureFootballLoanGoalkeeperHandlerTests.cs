using Application.Common;
using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Teams.Handlers;
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

public class EnsureFootballLoanGoalkeeperHandlerTests
{
    private readonly Mock<IFootballTeamRepository> _teams = new();
    private readonly Mock<IFootballPlayerRepository> _players = new();
    private readonly Mock<IPersonRepository> _persons = new();
    private readonly Mock<IUnitOfWork> _commonUnitOfWork = new();
    private readonly Mock<IFootballUnitOfWork> _footballUnitOfWork = new();
    private FootballPlayer? _createdPlayer;

    public EnsureFootballLoanGoalkeeperHandlerTests()
    {
        _persons.Setup(r => r.AddAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
        _commonUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _footballUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _players.Setup(r => r.AddAsync(It.IsAny<FootballPlayer>()))
            .Callback<FootballPlayer>(player => _createdPlayer = player)
            .Returns(Task.CompletedTask);
        _players.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<Guid> ids, CancellationToken _) =>
            {
                Dictionary<Guid, FootballPlayer> found = new();
                if (_createdPlayer != null && ids.Contains(_createdPlayer.Id))
                {
                    found[_createdPlayer.Id] = _createdPlayer;
                }
                return Task.FromResult(found);
            });
    }

    [Fact]
    public async Task Handle_CalledTwice_ReusesTheSamePlayer()
    {
        FootballTeam team = CreateTeam();
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        EnsureFootballLoanGoalkeeperCommand command = new(team.Id, Guid.NewGuid());
        EnsureFootballLoanGoalkeeperHandler handler = CreateHandler();

        Result<FootballLoanGoalkeeperDto> first = await handler.Handle(command, CancellationToken.None);
        Result<FootballLoanGoalkeeperDto> second = await handler.Handle(command, CancellationToken.None);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Data!.PlayerId.Should().Be(first.Data!.PlayerId);
        team.Roster.Should().ContainSingle(row =>
            row.PlayerId == first.Data.PlayerId && row.Position == FootballPosition.Goalkeeper);
        _players.Verify(r => r.AddAsync(It.IsAny<FootballPlayer>()), Times.Once);
    }

    private EnsureFootballLoanGoalkeeperHandler CreateHandler() => new(
        _teams.Object,
        _players.Object,
        _persons.Object,
        _commonUnitOfWork.Object,
        _footballUnitOfWork.Object,
        Mock.Of<ILogger<EnsureFootballLoanGoalkeeperHandler>>());

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
