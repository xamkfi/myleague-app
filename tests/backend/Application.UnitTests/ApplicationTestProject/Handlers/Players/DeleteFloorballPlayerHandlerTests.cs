using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Floorball.Players.Commands;
using Application.Features.Floorball.Players.Handlers;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Players;

public class DeleteFloorballPlayerHandlerTests
{
    private readonly Mock<IFloorballPlayerRepository> _playerRepository = new();
    private readonly Mock<IFloorballTeamRepository> _teamRepository = new();
    private readonly Mock<IFloorballUnitOfWork> _unitOfWork = new();
    private readonly DeleteFloorballPlayerHandler _handler;

    public DeleteFloorballPlayerHandlerTests()
    {
        _handler = new DeleteFloorballPlayerHandler(
            _playerRepository.Object,
            _teamRepository.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteFloorballPlayerHandler>>());
    }

    [Fact]
    public async Task Handle_PlayerNotFound_ReturnsNotFound()
    {
        Guid playerId = Guid.NewGuid();
        _playerRepository.Setup(x => x.ExistsAsync(playerId)).ReturnsAsync(false);

        Result result = await _handler.Handle(new DeleteFloorballPlayerCommand(playerId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _playerRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PlayerHasHistory_ReturnsFailure()
    {
        Guid playerId = Guid.NewGuid();
        _playerRepository.Setup(x => x.ExistsAsync(playerId)).ReturnsAsync(true);
        _playerRepository.Setup(x => x.HasCompetitionHistoryAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result result = await _handler.Handle(new DeleteFloorballPlayerCommand(playerId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeletionReasons.PlayerHasHistory);
        _playerRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnusedRosterOnlyPlayer_RemovesRosterThenDeletes()
    {
        Guid playerId = Guid.NewGuid();
        _playerRepository.Setup(x => x.ExistsAsync(playerId)).ReturnsAsync(true);
        _playerRepository.Setup(x => x.HasCompetitionHistoryAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _teamRepository.Setup(x => x.GetTeamsByPlayerIdAsync(playerId))
            .ReturnsAsync(Array.Empty<FloorballTeam>());

        Result result = await _handler.Handle(new DeleteFloorballPlayerCommand(playerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _playerRepository.Verify(x => x.DeleteAsync(playerId), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
