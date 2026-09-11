using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Football.Players.Commands;
using Application.Features.Football.Players.Handlers;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Players;

public class DeleteFootballPlayerHandlerTests
{
    private readonly Mock<IFootballPlayerRepository> _playerRepository = new();
    private readonly Mock<IFootballTeamRepository> _teamRepository = new();
    private readonly Mock<IFootballUnitOfWork> _unitOfWork = new();
    private readonly DeleteFootballPlayerHandler _handler;

    public DeleteFootballPlayerHandlerTests()
    {
        _handler = new DeleteFootballPlayerHandler(
            _playerRepository.Object,
            _teamRepository.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteFootballPlayerHandler>>());
    }

    [Fact]
    public async Task Handle_PlayerHasHistory_ReturnsFailure()
    {
        Guid playerId = Guid.NewGuid();
        _playerRepository.Setup(x => x.ExistsAsync(playerId)).ReturnsAsync(true);
        _playerRepository.Setup(x => x.HasCompetitionHistoryAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result result = await _handler.Handle(new DeleteFootballPlayerCommand(playerId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeletionReasons.PlayerHasHistory);
        _playerRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnusedRosterOnlyPlayer_Deletes()
    {
        Guid playerId = Guid.NewGuid();
        _playerRepository.Setup(x => x.ExistsAsync(playerId)).ReturnsAsync(true);
        _playerRepository.Setup(x => x.HasCompetitionHistoryAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _teamRepository.Setup(x => x.GetTeamsByPlayerIdAsync(playerId))
            .ReturnsAsync(Array.Empty<FootballTeam>());

        Result result = await _handler.Handle(new DeleteFootballPlayerCommand(playerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _playerRepository.Verify(x => x.DeleteAsync(playerId), Times.Once);
    }
}
