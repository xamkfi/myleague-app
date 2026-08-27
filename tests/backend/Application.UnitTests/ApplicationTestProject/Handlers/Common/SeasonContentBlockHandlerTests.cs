using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.SeasonContentBlocks.Commands;
using Application.Features.Common.SeasonContentBlocks.Queries;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Moq;
using SeasonContentBlockEntity = Domain.Entities.Common.SeasonContentBlock;

namespace ApplicationTestProject.Handlers.Common;

public class SeasonContentBlockHandlerTests
{
    private readonly Mock<ISeasonContentBlockRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly Guid CompetitionId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public async Task Create_ValidCommand_AddsAndSaves()
    {
        CreateSeasonContentBlockCommandHandler handler = new(_repo.Object, _uow.Object);

        Result<SeasonContentBlockDto> result = await handler.Handle(
            new CreateSeasonContentBlockCommand(
                SportsCategory.Floorball,
                CompetitionId,
                "2025-2026",
                "Sarjainfo",
                "<p>Info</p>",
                1,
                "admin"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be("Sarjainfo");
        result.Data.Sport.Should().Be(SportsCategory.Floorball);
        result.Data.SeasonYear.Should().Be("2025-2026");
        _repo.Verify(r => r.AddAsync(It.IsAny<SeasonContentBlockEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        GetSeasonContentBlockByIdQueryHandler handler = new(_repo.Object);
        Guid id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SeasonContentBlockEntity?)null);

        Result<SeasonContentBlockDto> result = await handler.Handle(
            new GetSeasonContentBlockByIdQuery(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("SeasonContentBlock");
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetAll_ByCompetition_ReturnsOrderedDtos()
    {
        GetAllSeasonContentBlocksQueryHandler handler = new(_repo.Object);
        _repo.Setup(r => r.GetByCompetitionIdAsync(CompetitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                CreateBlock("Intro", 0),
                CreateBlock("Sarjainfo", 1)
            ]);

        Result<IReadOnlyList<SeasonContentBlockDto>> result = await handler.Handle(
            new GetAllSeasonContentBlocksQuery(CompetitionId, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].Title.Should().Be("Intro");
        result.Data[1].Title.Should().Be("Sarjainfo");
    }

    [Fact]
    public async Task GetAll_BySportAndYear_UsesYearFilter()
    {
        GetAllSeasonContentBlocksQueryHandler handler = new(_repo.Object);
        _repo.Setup(r => r.GetBySportAndSeasonYearAsync(
                SportsCategory.Football,
                "2026",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateBlock("Football intro", 0, SportsCategory.Football, "2026")]);

        Result<IReadOnlyList<SeasonContentBlockDto>> result = await handler.Handle(
            new GetAllSeasonContentBlocksQuery(null, SportsCategory.Football, "2026"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].Sport.Should().Be(SportsCategory.Football);
        _repo.Verify(r => r.GetByCompetitionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        UpdateSeasonContentBlockCommandHandler handler = new(_repo.Object, _uow.Object);
        Guid id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SeasonContentBlockEntity?)null);

        Result<SeasonContentBlockDto> result = await handler.Handle(
            new UpdateSeasonContentBlockCommand(id, "Title", "<p>x</p>", 0, "admin"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _repo.Verify(r => r.UpdateAsync(It.IsAny<SeasonContentBlockEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenExists_UpdatesContent()
    {
        UpdateSeasonContentBlockCommandHandler handler = new(_repo.Object, _uow.Object);
        SeasonContentBlockEntity entity = CreateBlock("Old", 0);
        _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        Result<SeasonContentBlockDto> result = await handler.Handle(
            new UpdateSeasonContentBlockCommand(entity.Id, "New", "<p>new</p>", 2, "editor"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be("New");
        entity.ContentHtml.Should().Be("<p>new</p>");
        entity.SortOrder.Should().Be(2);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenExists_RemovesAndSaves()
    {
        DeleteSeasonContentBlockCommandHandler handler = new(_repo.Object, _uow.Object);
        SeasonContentBlockEntity entity = CreateBlock("Intro", 0);
        _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        Result<bool> result = await handler.Handle(
            new DeleteSeasonContentBlockCommand(entity.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.RemoveAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reorder_SameCompetition_AssignsSequentialSortOrder()
    {
        ReorderSeasonContentBlocksCommandHandler handler = new(_repo.Object, _uow.Object);
        SeasonContentBlockEntity first = CreateBlock("A", 5);
        SeasonContentBlockEntity second = CreateBlock("B", 1);
        _repo.Setup(r => r.GetByIdAsync(first.Id, It.IsAny<CancellationToken>())).ReturnsAsync(first);
        _repo.Setup(r => r.GetByIdAsync(second.Id, It.IsAny<CancellationToken>())).ReturnsAsync(second);

        Result<IReadOnlyList<SeasonContentBlockDto>> result = await handler.Handle(
            new ReorderSeasonContentBlocksCommand([second.Id, first.Id], "admin"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        second.SortOrder.Should().Be(0);
        first.SortOrder.Should().Be(1);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reorder_MixedCompetitions_ReturnsFailure()
    {
        ReorderSeasonContentBlocksCommandHandler handler = new(_repo.Object, _uow.Object);
        SeasonContentBlockEntity first = CreateBlock("A", 0);
        SeasonContentBlockEntity second = CreateBlock(
            "B",
            1,
            SportsCategory.Floorball,
            "2025-2026",
            Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(first.Id, It.IsAny<CancellationToken>())).ReturnsAsync(first);
        _repo.Setup(r => r.GetByIdAsync(second.Id, It.IsAny<CancellationToken>())).ReturnsAsync(second);

        Result<IReadOnlyList<SeasonContentBlockDto>> result = await handler.Handle(
            new ReorderSeasonContentBlocksCommand([first.Id, second.Id], "admin"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("same season");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static SeasonContentBlockEntity CreateBlock(
        string title,
        int sortOrder,
        SportsCategory sport = SportsCategory.Floorball,
        string seasonYear = "2025-2026",
        Guid? competitionId = null)
    {
        return new SeasonContentBlockEntity(
            Guid.NewGuid(),
            sport,
            competitionId ?? CompetitionId,
            seasonYear,
            title,
            $"<p>{title}</p>",
            sortOrder,
            "admin");
    }
}
