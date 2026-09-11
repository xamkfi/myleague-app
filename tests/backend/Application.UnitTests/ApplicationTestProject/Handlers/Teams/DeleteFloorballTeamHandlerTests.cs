using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.Handlers;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Teams;

public class DeleteFloorballTeamHandlerTests
{
    private readonly Mock<IFloorballTeamRepository> _teamRepository = new();
    private readonly Mock<IFloorballMatchRepository> _matchRepository = new();
    private readonly Mock<IFloorballUnitOfWork> _unitOfWork = new();
    private readonly DeleteFloorballTeamHandler _handler;

    public DeleteFloorballTeamHandlerTests()
    {
        _handler = new DeleteFloorballTeamHandler(
            _teamRepository.Object,
            _matchRepository.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteFloorballTeamHandler>>());
    }

    [Fact]
    public async Task Handle_TeamNotFound_ReturnsNotFound()
    {
        Guid teamId = Guid.NewGuid();
        _teamRepository.Setup(x => x.ExistsAsync(teamId)).ReturnsAsync(false);

        Result result = await _handler.Handle(new DeleteFloorballTeamCommand(teamId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _teamRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TeamUsedInMatches_ReturnsFailure()
    {
        Guid teamId = Guid.NewGuid();
        _teamRepository.Setup(x => x.ExistsAsync(teamId)).ReturnsAsync(true);
        _matchRepository.Setup(x => x.HasAnyForTeamAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result result = await _handler.Handle(new DeleteFloorballTeamCommand(teamId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeletionReasons.TeamUsedInMatches);
        _teamRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TeamHasNoMatches_Deletes()
    {
        Guid teamId = Guid.NewGuid();
        _teamRepository.Setup(x => x.ExistsAsync(teamId)).ReturnsAsync(true);
        _matchRepository.Setup(x => x.HasAnyForTeamAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Result result = await _handler.Handle(new DeleteFloorballTeamCommand(teamId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _teamRepository.Verify(x => x.DeleteAsync(teamId), Times.Once);
    }
}
