using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.InfoPageContent.Commands;
using Application.Features.Common.InfoPageContent.Queries;
using Domain.Repositories.Common;
using Moq;
using InfoPageEntity = Domain.Entities.Common.InfoPageContent;

namespace ApplicationTestProject.Handlers.Common;

public class InfoPageContentHandlerTests
{
    private readonly Mock<IInfoPageContentRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    [Fact]
    public async Task GetBySlug_WhenMissing_ReturnsNotFound()
    {
        GetInfoPageContentBySlugQueryHandler handler = new(_repo.Object);
        _repo.Setup(r => r.GetBySlugAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InfoPageEntity?)null);

        Result<InfoPageContentDto> result = await handler.Handle(
            new GetInfoPageContentBySlugQuery("missing"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("InfoPageContent");
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetBySlug_WhenFound_ReturnsDto()
    {
        GetInfoPageContentBySlugQueryHandler handler = new(_repo.Object);
        InfoPageEntity entity = new(Guid.NewGuid(), "about", "About", "<p>Hi</p>");
        _repo.Setup(r => r.GetBySlugAsync("about", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        Result<InfoPageContentDto> result = await handler.Handle(
            new GetInfoPageContentBySlugQuery("about"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PageSlug.Should().Be("about");
        result.Data.Title.Should().Be("About");
    }

    [Fact]
    public async Task GetAll_ReturnsMappedList()
    {
        GetAllInfoPageContentsQueryHandler handler = new(_repo.Object);
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new InfoPageEntity(Guid.NewGuid(), "a", "A", "<p>a</p>"),
                new InfoPageEntity(Guid.NewGuid(), "b", "B", "<p>b</p>")
            ]);

        Result<IReadOnlyList<InfoPageContentDto>> result = await handler.Handle(
            new GetAllInfoPageContentsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Update_WhenMissing_CreatesEntity()
    {
        UpdateInfoPageContentCommandHandler handler = new(_repo.Object, _uow.Object);
        _repo.Setup(r => r.GetBySlugAsync("new-page", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InfoPageEntity?)null);

        Result<InfoPageContentDto> result = await handler.Handle(
            new UpdateInfoPageContentCommand("new-page", "Title", "<p>body</p>", "admin"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PageSlug.Should().Be("new-page");
        _repo.Verify(r => r.AddAsync(It.IsAny<InfoPageEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenExists_UpdatesContent()
    {
        UpdateInfoPageContentCommandHandler handler = new(_repo.Object, _uow.Object);
        InfoPageEntity entity = new(Guid.NewGuid(), "home", "Old", "<p>old</p>");
        _repo.Setup(r => r.GetBySlugAsync("home", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        Result<InfoPageContentDto> result = await handler.Handle(
            new UpdateInfoPageContentCommand("home", "New", "<p>new</p>", "editor"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be("New");
        entity.ContentHtml.Should().Be("<p>new</p>");
        _repo.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddAsync(It.IsAny<InfoPageEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
