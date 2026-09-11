using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Controllers.Floorball;
using WebAPI.Models.Common;

namespace WebApiTestProject.Controllers.Floorball;

public class FloorballMatchesControllerTests
{
    [Fact]
    public async Task GetMatchById_WhenNotFound_Returns404()
    {
        Mock<IMediator> mediator = new();
        FloorballMatchesController controller = new(
            mediator.Object,
            Mock.Of<ILogger<FloorballMatchesController>>());

        Guid id = Guid.NewGuid();
        mediator
            .Setup(m => m.Send(It.IsAny<GetFloorballMatchByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FloorballMatchDto>.NotFound("FloorballMatch", id));

        ActionResult<ApiResponse<FloorballMatchDto>> actionResult =
            await controller.GetMatchById(id, CancellationToken.None);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMatchById_WhenFailure_Returns400()
    {
        Mock<IMediator> mediator = new();
        FloorballMatchesController controller = new(
            mediator.Object,
            Mock.Of<ILogger<FloorballMatchesController>>());

        mediator
            .Setup(m => m.Send(It.IsAny<GetFloorballMatchByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FloorballMatchDto>.Failure("Match is cancelled."));

        ActionResult<ApiResponse<FloorballMatchDto>> actionResult =
            await controller.GetMatchById(Guid.NewGuid(), CancellationToken.None);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
