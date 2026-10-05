using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class EnsureHockeyLoanGoalkeeperHandlerTests
{
    private readonly Mock<IHockeyTeamRepository> _teams = new();
    private readonly Mock<IHockeyPlayerRepository> _players = new();
    private readonly Mock<IPersonRepository> _persons = new();
    private readonly Mock<IUnitOfWork> _commonUnitOfWork = new();
    private readonly Mock<IHockeyUnitOfWork> _hockeyUnitOfWork = new();
    private HockeyPlayer? _createdPlayer;

    public EnsureHockeyLoanGoalkeeperHandlerTests()
    {
        _persons.Setup(r => r.AddAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
        _commonUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _hockeyUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _players.Setup(r => r.AddAsync(It.IsAny<HockeyPlayer>()))
            .Callback<HockeyPlayer>(player => _createdPlayer = player)
            .Returns(Task.CompletedTask);
        _players.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<Guid> ids, CancellationToken _) =>
            {
                Dictionary<Guid, HockeyPlayer> found = new();
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
        Club club = new("Test Club");
        HockeyTeam team = new("Wolves", club, TeamCategory.Adult, shortName: "WOL");
        _teams.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        EnsureHockeyLoanGoalkeeperCommand command = new(team.Id, Guid.NewGuid());
        EnsureHockeyLoanGoalkeeperHandler handler = CreateHandler();

        Result<HockeyLoanGoalkeeperDto> first = await handler.Handle(command, CancellationToken.None);
        Result<HockeyLoanGoalkeeperDto> second = await handler.Handle(command, CancellationToken.None);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Data!.PlayerId.Should().Be(first.Data!.PlayerId);
        second.Data.RosterEntryId.Should().Be(first.Data.RosterEntryId);
        team.Roster.Should().ContainSingle(row =>
            row.PlayerId == first.Data.PlayerId && row.Position == HockeyPosition.Goalie && row.IsActive);
        _players.Verify(r => r.AddAsync(It.IsAny<HockeyPlayer>()), Times.Once);
    }

    private EnsureHockeyLoanGoalkeeperHandler CreateHandler() => new(
        _teams.Object,
        _players.Object,
        _persons.Object,
        _commonUnitOfWork.Object,
        _hockeyUnitOfWork.Object,
        Mock.Of<ILogger<EnsureHockeyLoanGoalkeeperHandler>>());
}
