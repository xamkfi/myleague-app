using Application.Common;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.Handlers;
using Application.Features.Hockey.Seasons.Validators;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class DeleteHockeySeasonHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyMatchRepository> _matchRepo = new();
    private readonly Mock<IHockeyStatisticsRepository> _statsRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    private DeleteHockeySeasonHandler CreateHandler() => new(
        _competitionRepo.Object,
        _matchRepo.Object,
        _statsRepo.Object,
        _unitOfWork.Object,
        Mock.Of<ILogger<DeleteHockeySeasonHandler>>());

    [Fact]
    public async Task Handle_WhenMatchesAreUnplayed_DeletesMatchesStatsAndSeason()
    {
        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _matchRepo
            .Setup(r => r.HasMatchThatBlocksSeasonDeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _matchRepo
            .Setup(r => r.DeleteAllByCompetitionIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        Result result = await CreateHandler().Handle(new DeleteHockeySeasonCommand(id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _matchRepo.Verify(r => r.DeleteAllByCompetitionIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _statsRepo.Verify(r => r.ResetCompetitionStatisticsAsync(id, null, null, null, null), Times.Once);
        _competitionRepo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMatchHasStarted_ReturnsFailure()
    {
        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _matchRepo
            .Setup(r => r.HasMatchThatBlocksSeasonDeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result result = await CreateHandler().Handle(new DeleteHockeySeasonCommand(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("started, finished, or been cancelled");
        _competitionRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _statsRepo.Verify(
            r => r.ResetCompetitionStatisticsAsync(It.IsAny<Guid>(), null, null, null, null),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSeasonMissing_ReturnsNotFound()
    {
        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        Result result = await CreateHandler().Handle(new DeleteHockeySeasonCommand(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _matchRepo.Verify(
            r => r.HasMatchThatBlocksSeasonDeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Validator_EmptyId_IsInvalid()
    {
        DeleteHockeySeasonCommandValidator validator = new();
        FluentValidation.Results.ValidationResult result = validator.Validate(new DeleteHockeySeasonCommand(Guid.Empty));
        result.IsValid.Should().BeFalse();
    }
}
