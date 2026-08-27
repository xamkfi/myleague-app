using Application.Common;
using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Handlers;
using Application.Features.Floorball.Seasons.Queries;
using Application.Features.Floorball.Seasons.Validators;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class FloorballSeasonContentBlockHandlerTests
{
    private readonly Mock<IFloorballCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IFloorballUnitOfWork> _unitOfWork = new();

    private static FloorballSeason CreateSeason() =>
        new(
            "Championship 2026",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        GetFloorballSeasonContentBlocksHandler handler = new(
            _competitionRepo.Object,
            Mock.Of<ILogger<GetFloorballSeasonContentBlocksHandler>>());
        Guid id = Guid.NewGuid();
        _competitionRepo
            .Setup(repo => repo.GetSeasonWithContentBlocksAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FloorballSeason?)null);

        Result<FloorballSeasonContentBlocksDto> result = await handler.Handle(
            new GetFloorballSeasonContentBlocksQuery(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOrderedBlocks()
    {
        FloorballSeason season = CreateSeason();
        season.ReplaceContentBlocks([(null, "Second", "<p>2</p>"), (null, "First", "<p>1</p>")]);
        season.ContentBlocks.First(block => block.Title == "Second").Update("Second", "<p>2</p>", 1);
        season.ContentBlocks.First(block => block.Title == "First").Update("First", "<p>1</p>", 0);
        _competitionRepo
            .Setup(repo => repo.GetSeasonWithContentBlocksAsync(season.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(season);

        GetFloorballSeasonContentBlocksHandler handler = new(
            _competitionRepo.Object,
            Mock.Of<ILogger<GetFloorballSeasonContentBlocksHandler>>());

        Result<FloorballSeasonContentBlocksDto> result = await handler.Handle(
            new GetFloorballSeasonContentBlocksQuery(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.SeasonId.Should().Be(season.Id);
        result.Data.Blocks.Select(block => block.Title).Should().Equal("First", "Second");
    }

    [Fact]
    public async Task Replace_WhenMissing_ReturnsNotFound()
    {
        ReplaceFloorballSeasonContentBlocksHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<ReplaceFloorballSeasonContentBlocksHandler>>());
        Guid id = Guid.NewGuid();
        _competitionRepo
            .Setup(repo => repo.GetSeasonWithContentBlocksAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FloorballSeason?)null);

        Result<FloorballSeasonContentBlocksDto> result = await handler.Handle(
            new ReplaceFloorballSeasonContentBlocksCommand(id, []),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Replace_ValidItems_SavesBlocks()
    {
        FloorballSeason season = CreateSeason();
        _competitionRepo
            .Setup(repo => repo.GetSeasonWithContentBlocksAsync(season.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(season);

        ReplaceFloorballSeasonContentBlocksHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<ReplaceFloorballSeasonContentBlocksHandler>>());

        Result<FloorballSeasonContentBlocksDto> result = await handler.Handle(
            new ReplaceFloorballSeasonContentBlocksCommand(
                season.Id,
                [new ReplaceFloorballSeasonContentBlockItem(null, "Intro", "<p>Hello</p>")]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Blocks.Should().ContainSingle(block => block.Title == "Intro");
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ReplaceValidator_EmptyTitle_HasError()
    {
        ReplaceFloorballSeasonContentBlocksCommandValidator validator = new();
        ReplaceFloorballSeasonContentBlocksCommand command = new(
            Guid.NewGuid(),
            [new ReplaceFloorballSeasonContentBlockItem(null, " ", "<p>x</p>")]);

        TestValidationResult<ReplaceFloorballSeasonContentBlocksCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].Title");
    }
}
