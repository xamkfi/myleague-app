using Application.Common;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Players.Handlers;
using Application.Features.Floorball.Players.Queries;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Common;
using Domain.ValueObjects.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Players;

public class GetFloorballPlayerByIdHandlerTests
{
    private readonly Person _person = new(
        "Matti",
        "Pelaaja",
        new DateTime(1998, 3, 12, 0, 0, 0, DateTimeKind.Utc),
        PersonRole.User,
        new Address("Katu 1", "Mikkeli", "50100", "Finland"),
        new ContactInfo("matti@example.com", "+358401234567"));

    private readonly FloorballPlayer _player;
    private readonly GetFloorballPlayerByIdHandler _handler;

    public GetFloorballPlayerByIdHandlerTests()
    {
        _player = new FloorballPlayer(_person.Id, new Position(FloorballPosition.Forward));

        Mock<IFloorballPlayerRepository> players = new();
        players.Setup(r => r.GetByIdAsync(_player.Id)).ReturnsAsync(_player);
        Mock<IPersonRepository> persons = new();
        persons.Setup(r => r.GetByIdAsync(_person.Id)).ReturnsAsync(_person);

        _handler = new GetFloorballPlayerByIdHandler(
            players.Object,
            persons.Object,
            new Mock<ILogger<GetFloorballPlayerByIdHandler>>().Object);
    }

    [Fact]
    public async Task Handle_WithoutPrivateDataAccess_RedactsPersonalDetails()
    {
        Result<FloorballPlayerDto> result = await _handler.Handle(
            new GetFloorballPlayerByIdQuery(_player.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Person.FullName.Should().Be(_person.FullName);
        result.Data.Person.BirthDate.Should().BeNull();
        result.Data.Person.Address.Should().BeNull();
        result.Data.Person.ContactInfo.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithPrivateDataAccess_ReturnsPersonalDetails()
    {
        Result<FloorballPlayerDto> result = await _handler.Handle(
            new GetFloorballPlayerByIdQuery(_player.Id, IncludePrivateData: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Person.BirthDate.Should().Be(_person.BirthDate);
        result.Data.Person.Address.Should().NotBeNull();
        result.Data.Person.ContactInfo.Should().NotBeNull();
    }
}
