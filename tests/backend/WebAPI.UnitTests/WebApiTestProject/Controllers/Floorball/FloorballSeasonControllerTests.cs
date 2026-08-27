using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Enums.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Controllers.Floorball;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebApiTestProject.Controllers.Floorball;

public class FloorballSeasonControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly FloorballSeasonController _controller;

    public FloorballSeasonControllerTests()
    {
        _controller = new FloorballSeasonController(
            _mediator.Object,
            Mock.Of<ILogger<FloorballSeasonController>>());
    }

    private static FloorballSeasonDto CreateSeasonDto(Guid id) =>
        new(
            id,
            "Championship 2026",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive: false,
            IsCompleted: false,
            SeasonDivisions: Array.Empty<FloorballSeasonDivisionDto>(),
            Teams: Array.Empty<FloorballTeamDto>(),
            Matches: Array.Empty<FloorballMatchDto>(),
            MatchRules: new FloorballMatchRulesDto(2, 15, true, 5, true),
            TeamCategory: TeamCategory.Adult);

    [Fact]
    public async Task GetSeasonById_WhenFound_ReturnsOk()
    {
        Guid id = Guid.NewGuid();
        FloorballSeasonDto dto = CreateSeasonDto(id);
        _mediator
            .Setup(m => m.Send(It.Is<GetFloorballSeasonByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FloorballSeasonDto>.Success(dto));

        ActionResult<ApiResponse<FloorballSeasonDto>> actionResult = await _controller.GetSeasonById(id);

        OkObjectResult ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<FloorballSeasonDto> body = ok.Value.Should().BeOfType<ApiResponse<FloorballSeasonDto>>().Subject;
        body.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetSeasonById_WhenNotFound_Returns404()
    {
        Guid id = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<GetFloorballSeasonByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FloorballSeasonDto>.NotFound("FloorballSeason", id));

        ActionResult<ApiResponse<FloorballSeasonDto>> actionResult = await _controller.GetSeasonById(id);

        NotFoundObjectResult notFound = actionResult.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        ApiResponse<FloorballSeasonDto> body = notFound.Value.Should().BeOfType<ApiResponse<FloorballSeasonDto>>().Subject;
        body.Success.Should().BeFalse();
        body.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetSeasonById_WhenFailure_Returns400()
    {
        Guid id = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<GetFloorballSeasonByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FloorballSeasonDto>.Failure("Season is locked."));

        ActionResult<ApiResponse<FloorballSeasonDto>> actionResult = await _controller.GetSeasonById(id);

        BadRequestObjectResult badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ApiResponse<FloorballSeasonDto> body = badRequest.Value.Should().BeOfType<ApiResponse<FloorballSeasonDto>>().Subject;
        body.Success.Should().BeFalse();
        body.Message.Should().Contain("locked");
    }

    [Fact]
    public async Task GetContentBlocks_WhenFound_ReturnsOk()
    {
        Guid id = Guid.NewGuid();
        FloorballSeasonContentBlocksDto dto = new(
            id,
            [new FloorballSeasonContentBlockDto(Guid.NewGuid(), "Intro", "<p>Hi</p>", 0)]);
        _mediator
            .Setup(m => m.Send(It.Is<GetFloorballSeasonContentBlocksQuery>(q => q.SeasonId == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FloorballSeasonContentBlocksDto>.Success(dto));

        ActionResult<ApiResponse<FloorballSeasonContentBlocksDto>> actionResult = await _controller.GetContentBlocks(id);

        OkObjectResult ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<FloorballSeasonContentBlocksDto> body = ok.Value.Should().BeOfType<ApiResponse<FloorballSeasonContentBlocksDto>>().Subject;
        body.Success.Should().BeTrue();
        body.Data!.SeasonId.Should().Be(id);
    }

    [Fact]
    public async Task ReplaceContentBlocks_WhenNotFound_Returns404()
    {
        Guid id = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<ReplaceFloorballSeasonContentBlocksCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FloorballSeasonContentBlocksDto>.NotFound("FloorballSeason", id));

        ActionResult<ApiResponse<FloorballSeasonContentBlocksDto>> actionResult =
            await _controller.ReplaceContentBlocks(
                id,
                new ReplaceFloorballSeasonContentBlocksRequest
                {
                    Items = [new FloorballSeasonContentBlockItemRequest { Title = "Intro", ContentHtml = "<p>Hi</p>" }],
                });

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
