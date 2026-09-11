using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Common.Persons.Commands;
using Application.Features.Common.Persons.Handlers;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Persons;

public class DeletePersonHandlerTests
{
    private readonly Mock<IPersonRepository> _personRepository = new();
    private readonly Mock<IPersonDeletionGuard> _guard = new();
    private readonly Mock<IFloorballPlayerRepository> _floorballPlayerRepository = new();
    private readonly Mock<IFootballPlayerRepository> _footballPlayerRepository = new();
    private readonly Mock<IHockeyPlayerRepository> _hockeyPlayerRepository = new();
    private readonly Mock<IFloorballTeamRepository> _floorballTeamRepository = new();
    private readonly Mock<IFootballTeamRepository> _footballTeamRepository = new();
    private readonly Mock<IFloorballRefereeRepository> _floorballRefereeRepository = new();
    private readonly Mock<IFootballRefereeRepository> _footballRefereeRepository = new();
    private readonly Mock<IHockeyOfficialRepository> _hockeyOfficialRepository = new();
    private readonly Mock<IFloorballTeamManagerRepository> _floorballTeamManagerRepository = new();
    private readonly Mock<IFootballTeamManagerRepository> _footballTeamManagerRepository = new();
    private readonly Mock<IFloorballUnitOfWork> _floorballUnitOfWork = new();
    private readonly Mock<IFootballUnitOfWork> _footballUnitOfWork = new();
    private readonly Mock<IHockeyUnitOfWork> _hockeyUnitOfWork = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeletePersonHandler _handler;

    public DeletePersonHandlerTests()
    {
        _handler = new DeletePersonHandler(
            _personRepository.Object,
            _guard.Object,
            _floorballPlayerRepository.Object,
            _footballPlayerRepository.Object,
            _hockeyPlayerRepository.Object,
            _floorballTeamRepository.Object,
            _footballTeamRepository.Object,
            _floorballRefereeRepository.Object,
            _footballRefereeRepository.Object,
            _hockeyOfficialRepository.Object,
            _floorballTeamManagerRepository.Object,
            _footballTeamManagerRepository.Object,
            _floorballUnitOfWork.Object,
            _footballUnitOfWork.Object,
            _hockeyUnitOfWork.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeletePersonHandler>>());
    }

    [Fact]
    public async Task Handle_PersonNotFound_ReturnsNotFound()
    {
        Guid personId = Guid.NewGuid();
        _personRepository.Setup(x => x.ExistsAsync(personId)).ReturnsAsync(false);

        Result result = await _handler.Handle(new DeletePersonCommand(personId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _personRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BlockedByGuard_ReturnsFailure()
    {
        Guid personId = Guid.NewGuid();
        _personRepository.Setup(x => x.ExistsAsync(personId)).ReturnsAsync(true);
        _guard.Setup(x => x.EvaluateAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonHasUserAccount });

        Result result = await _handler.Handle(new DeletePersonCommand(personId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeletionReasons.PersonHasUserAccount);
        _personRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnusedPerson_DeletesPerson()
    {
        Guid personId = Guid.NewGuid();
        _personRepository.Setup(x => x.ExistsAsync(personId)).ReturnsAsync(true);
        _guard.Setup(x => x.EvaluateAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonDeletionEvaluation());

        Result result = await _handler.Handle(new DeletePersonCommand(personId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _personRepository.Verify(x => x.DeleteAsync(personId), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnusedPlayerProfile_DeletesProfileThenPerson()
    {
        Guid personId = Guid.NewGuid();
        Guid playerId = Guid.NewGuid();
        _personRepository.Setup(x => x.ExistsAsync(personId)).ReturnsAsync(true);
        _guard.Setup(x => x.EvaluateAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonDeletionEvaluation { UnusedFloorballPlayerId = playerId });
        _floorballTeamRepository.Setup(x => x.GetTeamsByPlayerIdAsync(playerId))
            .ReturnsAsync(Array.Empty<FloorballTeam>());

        Result result = await _handler.Handle(new DeletePersonCommand(personId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _floorballPlayerRepository.Verify(x => x.DeleteAsync(playerId), Times.Once);
        _personRepository.Verify(x => x.DeleteAsync(personId), Times.Once);
    }
}
