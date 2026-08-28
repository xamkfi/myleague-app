using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Floorball.Referees.Commands;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.Referees.Handlers;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Referees;

public class DeleteFloorballRefereeHandlerTests
{
    private readonly Mock<IFloorballRefereeRepository> _refereeRepository = new();
    private readonly Mock<IPersonRepository> _personRepository = new();
    private readonly Mock<IFloorballUnitOfWork> _unitOfWork = new();
    private readonly DeleteFloorballRefereeHandler _handler;

    public DeleteFloorballRefereeHandlerTests()
    {
        _handler = new DeleteFloorballRefereeHandler(
            _refereeRepository.Object,
            _personRepository.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteFloorballRefereeHandler>>());
    }

    [Fact]
    public async Task Handle_RefereeAssignedToMatch_ReturnsFailure()
    {
        Person person = new Person("Matti", "Tuomari");
        FloorballReferee referee = new FloorballReferee(
            person.Id,
            DateTime.UtcNow.AddYears(-1),
            DateTime.UtcNow.AddYears(1));
        _refereeRepository.Setup(x => x.GetByIdAsync(referee.Id)).ReturnsAsync(referee);
        _personRepository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _refereeRepository.Setup(x => x.IsAssignedToAnyMatchAsync(referee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result<FloorballRefereeDto> result = await _handler.Handle(
            new DeleteFloorballRefereeCommand(referee.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeletionReasons.RefereeAssignedToMatch);
        _refereeRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RefereeNeverAssigned_Deletes()
    {
        Person person = new Person("Matti", "Tuomari");
        FloorballReferee referee = new FloorballReferee(
            person.Id,
            DateTime.UtcNow.AddYears(-1),
            DateTime.UtcNow.AddYears(1));
        _refereeRepository.Setup(x => x.GetByIdAsync(referee.Id)).ReturnsAsync(referee);
        _personRepository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _refereeRepository.Setup(x => x.IsAssignedToAnyMatchAsync(referee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Result<FloorballRefereeDto> result = await _handler.Handle(
            new DeleteFloorballRefereeCommand(referee.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _refereeRepository.Verify(x => x.DeleteAsync(referee.Id), Times.Once);
    }
}
