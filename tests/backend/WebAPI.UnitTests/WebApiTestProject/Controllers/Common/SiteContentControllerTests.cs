using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.FooterContacts.Queries;
using Application.Features.Common.InfoPageContent.Queries;
using Application.Features.Common.RulesSection.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;

namespace WebApiTestProject.Controllers.Common;

public class InfoPageContentControllerTests
{
    [Fact]
    public async Task GetBySlug_WhenNotFound_Returns404()
    {
        Mock<IMediator> mediator = new();
        InfoPageContentController controller = new(
            mediator.Object,
            Mock.Of<ILogger<InfoPageContentController>>());

        mediator
            .Setup(m => m.Send(It.IsAny<GetInfoPageContentBySlugQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InfoPageContentDto>.NotFound("InfoPageContent", "missing"));

        ActionResult<ApiResponse<InfoPageContentDto>> actionResult =
            await controller.GetInfoPageContentBySlug("missing");

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetBySlug_WhenFound_ReturnsOk()
    {
        Mock<IMediator> mediator = new();
        InfoPageContentController controller = new(
            mediator.Object,
            Mock.Of<ILogger<InfoPageContentController>>());

        InfoPageContentDto dto = new()
        {
            Id = Guid.NewGuid(),
            PageSlug = "about",
            Title = "About",
            ContentHtml = "<p>Hi</p>"
        };
        mediator
            .Setup(m => m.Send(It.IsAny<GetInfoPageContentBySlugQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InfoPageContentDto>.Success(dto));

        ActionResult<ApiResponse<InfoPageContentDto>> actionResult =
            await controller.GetInfoPageContentBySlug("about");

        OkObjectResult ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<InfoPageContentDto> body = ok.Value.Should().BeOfType<ApiResponse<InfoPageContentDto>>().Subject;
        body.Success.Should().BeTrue();
        body.Data!.PageSlug.Should().Be("about");
    }
}

public class RulesSectionControllerTests
{
    [Fact]
    public async Task GetSectionById_WhenNotFound_Returns404()
    {
        Mock<IMediator> mediator = new();
        RulesSectionController controller = new(
            mediator.Object,
            Mock.Of<ILogger<RulesSectionController>>());

        Guid id = Guid.NewGuid();
        mediator
            .Setup(m => m.Send(It.IsAny<GetRulesSectionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RulesSectionDto>.NotFound("RulesSection", id));

        ActionResult<ApiResponse<RulesSectionDto>> actionResult = await controller.GetSectionById(id);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}

public class FooterContactControllerTests
{
    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        Mock<IMediator> mediator = new();
        FooterContactController controller = new(
            mediator.Object,
            Mock.Of<ILogger<FooterContactController>>());

        Guid id = Guid.NewGuid();
        mediator
            .Setup(m => m.Send(It.IsAny<GetFooterContactByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FooterContactDto>.NotFound("FooterContact", id));

        ActionResult<ApiResponse<FooterContactDto>> actionResult = await controller.GetById(id);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAll_WhenFound_ReturnsOk()
    {
        Mock<IMediator> mediator = new();
        FooterContactController controller = new(
            mediator.Object,
            Mock.Of<ILogger<FooterContactController>>());

        FooterContactDto dto = new() { Id = Guid.NewGuid(), Title = "Office" };
        mediator
            .Setup(m => m.Send(It.IsAny<GetAllFooterContactsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<FooterContactDto>>.Success([dto]));

        ActionResult<ApiResponse<IReadOnlyList<FooterContactDto>>> actionResult = await controller.GetAll(null);

        OkObjectResult ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<IReadOnlyList<FooterContactDto>> body =
            ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<FooterContactDto>>>().Subject;
        body.Success.Should().BeTrue();
        body.Data.Should().HaveCount(1);
    }
}
