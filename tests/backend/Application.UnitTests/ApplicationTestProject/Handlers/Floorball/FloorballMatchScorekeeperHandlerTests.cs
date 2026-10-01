using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class FloorballMatchScorekeeperHandlerTests
{
    private readonly Mock<IFloorballMatchRepository> _matchRepository = new();
    private readonly Mock<IPersonRepository> _personRepository = new();
    private readonly Mock<IFloorballUnitOfWork> _unitOfWork = new();

    private static FloorballMatch CreateMatch()
    {
        FloorballSeason season = new(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        return new FloorballMatch(season, null, null, new DateTime(2027, 1, 15, 18, 0, 0, DateTimeKind.Utc), "Arena");
    }

    private AddFloorballScorekeeperToMatchHandler CreateAddHandler() =>
        new(_matchRepository.Object, _personRepository.Object, _unitOfWork.Object,
            Mock.Of<ILogger<AddFloorballScorekeeperToMatchHandler>>());

    [Fact]
    public async Task Handle_WithExistingPerson_AddsScorekeeperWithName()
    {
        FloorballMatch match = CreateMatch();
        Person person = new("Toimi", "Tsija");
        _matchRepository.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _personRepository.Setup(r => r.ExistsAsync(person.Id)).ReturnsAsync(true);
        _personRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new[] { person });

        Result<FloorballMatchDto> result = await CreateAddHandler().Handle(
            new AddFloorballScorekeeperToMatchCommand(match.Id, person.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Scorekeepers.Should().ContainSingle(s => s.Id == person.Id && s.Name == "Toimi Tsija");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPerson_ReturnsNotFound()
    {
        FloorballMatch match = CreateMatch();
        _matchRepository.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _personRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        Result<FloorballMatchDto> result = await CreateAddHandler().Handle(
            new AddFloorballScorekeeperToMatchCommand(match.Id, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorKind.Should().Be(ResultErrorKind.NotFound);
        match.Scorekeepers.Should().BeEmpty();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCancelledMatch_ReturnsFailure()
    {
        FloorballMatch match = CreateMatch();
        match.Cancel();
        _matchRepository.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _personRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        Result<FloorballMatchDto> result = await CreateAddHandler().Handle(
            new AddFloorballScorekeeperToMatchCommand(match.Id, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WithExistingScorekeeper_RemovesIt()
    {
        FloorballMatch match = CreateMatch();
        Guid personId = Guid.NewGuid();
        match.AddScorekeeper(personId);
        _matchRepository.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _personRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(Array.Empty<Person>());
        RemoveFloorballScorekeeperFromMatchHandler handler = new(
            _matchRepository.Object, _personRepository.Object, _unitOfWork.Object,
            Mock.Of<ILogger<RemoveFloorballScorekeeperFromMatchHandler>>());

        Result<FloorballMatchDto> result = await handler.Handle(
            new RemoveFloorballScorekeeperFromMatchCommand(match.Id, personId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Scorekeepers.Should().BeEmpty();
    }
}
