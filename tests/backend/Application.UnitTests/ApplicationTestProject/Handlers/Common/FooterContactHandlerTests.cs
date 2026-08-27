using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.FooterContacts.Commands;
using Application.Features.Common.FooterContacts.Queries;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Moq;

namespace ApplicationTestProject.Handlers.Common;

public class FooterContactHandlerTests
{
    private readonly Mock<IFooterContactRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        GetFooterContactByIdQueryHandler handler = new(_repo.Object);
        Guid id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FooterContact?)null);

        Result<FooterContactDto> result = await handler.Handle(
            new GetFooterContactByIdQuery(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("FooterContact");
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetAll_ReturnsMappedList()
    {
        GetAllFooterContactsQueryHandler handler = new(_repo.Object);
        _repo.Setup(r => r.GetAllAsync(It.IsAny<FooterSection?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new FooterContact(Guid.NewGuid(), "A", null, "a@mahl.fi", null, null, 0),
                new FooterContact(Guid.NewGuid(), "B", null, null, "123", null, 1)
            ]);

        Result<IReadOnlyList<FooterContactDto>> result = await handler.Handle(
            new GetAllFooterContactsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].Title.Should().Be("A");
    }

    [Fact]
    public async Task GetAll_WhenSectionProvided_PassesFilter()
    {
        GetAllFooterContactsQueryHandler handler = new(_repo.Object);
        _repo.Setup(r => r.GetAllAsync(FooterSection.SeasonalSports, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new FooterContact(
                    Guid.NewGuid(),
                    "Jalkapallo",
                    null,
                    null,
                    null,
                    null,
                    0,
                    FooterSection.SeasonalSports)
            ]);

        Result<IReadOnlyList<FooterContactDto>> result = await handler.Handle(
            new GetAllFooterContactsQuery(FooterSection.SeasonalSports),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].Section.Should().Be(FooterSection.SeasonalSports);
        result.Data[0].Title.Should().Be("Jalkapallo");
    }

    [Fact]
    public async Task Create_WhenValid_AddsEntity()
    {
        CreateFooterContactCommandHandler handler = new(_repo.Object, _uow.Object);

        Result<FooterContactDto> result = await handler.Handle(
            new CreateFooterContactCommand(
                "Office",
                null,
                "office@mahl.fi",
                null,
                "https://mahl.fi",
                0,
                FooterSection.Contact,
                "admin"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be("Office");
        result.Data.Email.Should().Be("office@mahl.fi");
        result.Data.Section.Should().Be(FooterSection.Contact);
        _repo.Verify(r => r.AddAsync(It.IsAny<FooterContact>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenLinkSection_AddsEntity()
    {
        CreateFooterContactCommandHandler handler = new(_repo.Object, _uow.Object);

        Result<FooterContactDto> result = await handler.Handle(
            new CreateFooterContactCommand(
                "Jalkapallo",
                null,
                null,
                null,
                "https://mahl.fi/jalkapallo",
                0,
                FooterSection.SeasonalSports,
                "admin"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be("Jalkapallo");
        result.Data.Section.Should().Be(FooterSection.SeasonalSports);
        result.Data.Url.Should().Be("https://mahl.fi/jalkapallo");
        result.Data.Email.Should().BeNull();
        _repo.Verify(r => r.AddAsync(It.IsAny<FooterContact>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        UpdateFooterContactCommandHandler handler = new(_repo.Object, _uow.Object);
        Guid id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FooterContact?)null);

        Result<FooterContactDto> result = await handler.Handle(
            new UpdateFooterContactCommand(id, "Office", null, null, null, null, 0, FooterSection.Contact, "admin"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenExists_RemovesEntity()
    {
        DeleteFooterContactCommandHandler handler = new(_repo.Object, _uow.Object);
        FooterContact entity = new(Guid.NewGuid(), "Office", null, null, null, null, 0);
        _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        Result<bool> result = await handler.Handle(
            new DeleteFooterContactCommand(entity.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.RemoveAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
