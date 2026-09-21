using Application.Common;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Persons.Handlers;
using Application.Features.Common.Persons.Queries;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Floorball;
using Domain.Enums.Football;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using Domain.ValueObjects.Floorball;
using Domain.ValueObjects.Football;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Persons;

public class GetPersonPlayerSportsHandlerTests
{
    private readonly Mock<IPersonRepository> _personRepository = new();
    private readonly Mock<IFloorballPlayerRepository> _floorballPlayerRepository = new();
    private readonly Mock<IFootballPlayerRepository> _footballPlayerRepository = new();
    private readonly Mock<IHockeyPlayerRepository> _hockeyPlayerRepository = new();
    private readonly Mock<IFloorballTeamRepository> _floorballTeamRepository = new();
    private readonly Mock<IFootballTeamRepository> _footballTeamRepository = new();
    private readonly Mock<IHockeyTeamRepository> _hockeyTeamRepository = new();
    private readonly GetPersonPlayerSportsHandler _handler;

    public GetPersonPlayerSportsHandlerTests()
    {
        _floorballTeamRepository
            .Setup(x => x.GetOpenPlayerLicencesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerLicenceRow>());
        _footballTeamRepository
            .Setup(x => x.GetOpenPlayerLicencesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerLicenceRow>());
        _hockeyTeamRepository
            .Setup(x => x.GetOpenPlayerLicencesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerLicenceRow>());

        _handler = new GetPersonPlayerSportsHandler(
            _personRepository.Object,
            _floorballPlayerRepository.Object,
            _footballPlayerRepository.Object,
            _hockeyPlayerRepository.Object,
            _floorballTeamRepository.Object,
            _footballTeamRepository.Object,
            _hockeyTeamRepository.Object,
            Mock.Of<ILogger<GetPersonPlayerSportsHandler>>());
    }

    [Fact]
    public async Task Handle_PersonWithMultipleSports_ReturnsAllSports()
    {
        Person person = new Person("Matti", "Pelaaja", new DateTime(1998, 3, 12, 0, 0, 0, DateTimeKind.Utc));
        FloorballPlayer floorballPlayer = new FloorballPlayer(person.Id, new Position(FloorballPosition.Forward));
        FootballPlayer footballPlayer = new FootballPlayer(person.Id, new FootballPositionPreference(FootballPosition.Midfielder));
        HockeyPlayer hockeyPlayer = new HockeyPlayer(person.Id, HockeyPosition.Center);

        _personRepository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _floorballPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync(floorballPlayer);
        _footballPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync(footballPlayer);
        _hockeyPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync(hockeyPlayer);

        Result<PersonPlayerSportsDto> result = await _handler.Handle(
            new GetPersonPlayerSportsQuery(person.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PersonId.Should().Be(person.Id);
        result.Data.FullName.Should().Be("Matti Pelaaja");
        result.Data.BirthDate.Should().Be(person.BirthDate);
        result.Data.Sports.Should().HaveCount(3);
        result.Data.Sports.Should().Contain(s => s.Sport == "floorball" && s.PlayerId == floorballPlayer.Id);
        result.Data.Sports.Should().Contain(s => s.Sport == "football" && s.PlayerId == footballPlayer.Id);
        result.Data.Sports.Should().Contain(s => s.Sport == "hockey" && s.PlayerId == hockeyPlayer.Id);
        _floorballPlayerRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PersonWithOnlyFloorball_ReturnsSingleSport()
    {
        Person person = new Person("Liisa", "Salibandy");
        FloorballPlayer floorballPlayer = new FloorballPlayer(person.Id, new Position(FloorballPosition.Defender));

        _personRepository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _floorballPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync(floorballPlayer);
        _footballPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync((FootballPlayer?)null);
        _hockeyPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync((HockeyPlayer?)null);

        Result<PersonPlayerSportsDto> result = await _handler.Handle(
            new GetPersonPlayerSportsQuery(person.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Sports.Should().HaveCount(1);
        result.Data.Sports[0].Sport.Should().Be("floorball");
        result.Data.Sports[0].PlayerId.Should().Be(floorballPlayer.Id);
        result.Data.Licences.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PersonWithLicence_ReturnsLicenceRow()
    {
        Person person = new Person("Liisa", "Salibandy");
        FloorballPlayer floorballPlayer = new FloorballPlayer(person.Id, new Position(FloorballPosition.Defender));
        Guid teamId = Guid.NewGuid();
        Guid competitionId = Guid.NewGuid();

        _personRepository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _floorballPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync(floorballPlayer);
        _footballPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync((FootballPlayer?)null);
        _hockeyPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync((HockeyPlayer?)null);
        _floorballTeamRepository
            .Setup(x => x.GetOpenPlayerLicencesAsync(floorballPlayer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PlayerLicenceRow(teamId, "Happee", competitionId, "SM-sarja 2026", true)
            });

        Result<PersonPlayerSportsDto> result = await _handler.Handle(
            new GetPersonPlayerSportsQuery(person.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Licences.Should().ContainSingle();
        result.Data.Licences[0].Sport.Should().Be("floorball");
        result.Data.Licences[0].TeamName.Should().Be("Happee");
        result.Data.Licences[0].CompetitionName.Should().Be("SM-sarja 2026");
        result.Data.Licences[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FloorballPlayerId_ResolvesPersonAndSports()
    {
        Person person = new Person("Janne", "Maali");
        FloorballPlayer floorballPlayer = new FloorballPlayer(person.Id, new Position(FloorballPosition.Goalkeeper));

        _personRepository.Setup(x => x.GetByIdAsync(floorballPlayer.Id)).ReturnsAsync((Person?)null);
        _personRepository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _floorballPlayerRepository.Setup(x => x.GetByIdAsync(floorballPlayer.Id)).ReturnsAsync(floorballPlayer);
        _floorballPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync(floorballPlayer);
        _footballPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync((FootballPlayer?)null);
        _hockeyPlayerRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync((HockeyPlayer?)null);

        Result<PersonPlayerSportsDto> result = await _handler.Handle(
            new GetPersonPlayerSportsQuery(floorballPlayer.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PersonId.Should().Be(person.Id);
        result.Data.Sports.Should().ContainSingle(s => s.Sport == "floorball" && s.PlayerId == floorballPlayer.Id);
    }

    [Fact]
    public async Task Handle_UnknownId_ReturnsNotFound()
    {
        Guid unknownId = Guid.NewGuid();
        _personRepository.Setup(x => x.GetByIdAsync(unknownId)).ReturnsAsync((Person?)null);
        _floorballPlayerRepository.Setup(x => x.GetByIdAsync(unknownId)).ReturnsAsync((FloorballPlayer?)null);
        _footballPlayerRepository.Setup(x => x.GetByIdAsync(unknownId)).ReturnsAsync((FootballPlayer?)null);
        _hockeyPlayerRepository.Setup(x => x.GetByIdAsync(unknownId)).ReturnsAsync((HockeyPlayer?)null);

        Result<PersonPlayerSportsDto> result = await _handler.Handle(
            new GetPersonPlayerSportsQuery(unknownId),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Data.Should().BeNull();
    }
}
