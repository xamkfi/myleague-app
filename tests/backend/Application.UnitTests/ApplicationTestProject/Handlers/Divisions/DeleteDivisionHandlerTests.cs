using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Common.Divisions.Commands;
using Application.Features.Common.Divisions.Handlers;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Divisions;

public class DeleteDivisionHandlerTests
{
    private readonly Mock<IDivisionRepository> _divisionRepository = new();
    private readonly Mock<IFloorballTeamRepository> _floorballTeamRepository = new();
    private readonly Mock<IFootballTeamRepository> _footballTeamRepository = new();
    private readonly Mock<IHockeyTeamRepository> _hockeyTeamRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteDivisionHandler _handler;

    public DeleteDivisionHandlerTests()
    {
        _handler = new DeleteDivisionHandler(
            _divisionRepository.Object,
            _floorballTeamRepository.Object,
            _footballTeamRepository.Object,
            _hockeyTeamRepository.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteDivisionHandler>>());
    }

    [Fact]
    public async Task Handle_DivisionNotFound_ReturnsNotFound()
    {
        Guid divisionId = Guid.NewGuid();
        _divisionRepository.Setup(x => x.GetByIdAsync(divisionId)).ReturnsAsync((Division?)null);

        Result<bool> result = await _handler.Handle(new DeleteDivisionCommand(divisionId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _divisionRepository.Verify(x => x.DeleteAsync(It.IsAny<Division>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DivisionHasTeams_ReturnsFailure()
    {
        Division division = new Division("A-divisioona", "Test division", 1, SportsCategory.Floorball);
        _divisionRepository.Setup(x => x.GetByIdAsync(division.Id)).ReturnsAsync(division);
        _floorballTeamRepository.Setup(x => x.HasAnyForDivisionAsync(division.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result<bool> result = await _handler.Handle(new DeleteDivisionCommand(division.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeletionReasons.DivisionHasTeams);
        _divisionRepository.Verify(x => x.DeleteAsync(It.IsAny<Division>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DivisionHasNoTeams_Deletes()
    {
        Division division = new Division("A-divisioona", "Test division", 1, SportsCategory.Floorball);
        _divisionRepository.Setup(x => x.GetByIdAsync(division.Id)).ReturnsAsync(division);

        Result<bool> result = await _handler.Handle(new DeleteDivisionCommand(division.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _divisionRepository.Verify(x => x.DeleteAsync(division), Times.Once);
    }
}
