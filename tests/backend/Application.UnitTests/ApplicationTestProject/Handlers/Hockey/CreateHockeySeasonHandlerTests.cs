using Application.Common;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Handlers;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class CreateHockeySeasonHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<CreateHockeySeasonHandler>> _logger = new();
    private readonly CreateHockeySeasonHandler _handler;

    public CreateHockeySeasonHandlerTests()
    {
        _handler = new CreateHockeySeasonHandler(
            _competitionRepo.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsSeasonAndSaves()
    {
        CreateHockeySeasonCommand command = new(
            "Liiga 2026-27",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            "2026-27");

        Result<HockeySeasonDto> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Liiga 2026-27");
        result.Data.SeasonCode.Should().Be("2026-27");
        _competitionRepo.Verify(r => r.AddAsync(It.IsAny<HockeySeason>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
