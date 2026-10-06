using Application.Common;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Players.Handlers;
using Application.Features.Floorball.Players.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class GetAllFloorballPlayersHandlerTests
{
    [Fact]
    public async Task Handle_PageOfPlayers_LoadsPersonsInOneBatch()
    {
        Person first = new("Aino", "Virtanen");
        Person second = new("Eero", "Korhonen");
        FloorballPlayer firstPlayer = new(first.Id, new Position(FloorballPosition.Forward));
        FloorballPlayer secondPlayer = new(second.Id, new Position(FloorballPosition.Defender));

        Mock<IFloorballPlayerRepository> players = new();
        players
            .Setup(repository => repository.GetPagedWithTeamsAsync(
                1,
                25,
                It.IsAny<bool?>(),
                It.IsAny<FloorballPosition?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult.Create(
                new List<(FloorballPlayer Player, FloorballTeam? Team)> { (firstPlayer, null), (secondPlayer, null) },
                2,
                1,
                25));

        Mock<IFloorballTeamRepository> teams = new();
        teams
            .Setup(repository => repository.GetOpenPlayerLicencesByPlayerIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IReadOnlyList<PlayerLicenceRow>>());

        Mock<IPersonRepository> persons = new();
        persons
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Person> { first, second });

        Mock<IPaginationService> pagination = new();
        pagination
            .Setup(service => service.ResolvePageSize(It.IsAny<string>(), It.IsAny<int>()))
            .Returns<string, int>((_, size) => size == 0 ? 25 : size);
        pagination
            .Setup(service => service.IsValidPageSize(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(true);

        GetAllFloorballPlayersHandler handler = new(
            players.Object,
            teams.Object,
            persons.Object,
            pagination.Object,
            Mock.Of<ILogger<GetAllFloorballPlayersHandler>>());

        Result<PagedResult<FloorballPlayerDto>> result = await handler.Handle(
            new GetAllFloorballPlayersQuery(1, 25),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        persons.Verify(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        persons.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}
