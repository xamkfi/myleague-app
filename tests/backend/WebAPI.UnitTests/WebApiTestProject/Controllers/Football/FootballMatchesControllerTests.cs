using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Controllers.Football;
using WebAPI.Models.Common;

namespace WebApiTestProject.Controllers.Football;

public class FootballMatchesControllerTests
{
    [Fact]
    public async Task GetMatchById_WhenNotFound_Returns404()
    {
        Mock<IMediator> mediator = new();
        FootballMatchesController controller = new(
            mediator.Object,
            Mock.Of<ILogger<FootballMatchesController>>());

        Guid id = Guid.NewGuid();
        mediator
            .Setup(m => m.Send(It.IsAny<GetFootballMatchByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FootballMatchDto>.NotFound("FootballMatch", id));

        ActionResult<ApiResponse<FootballMatchDto>> actionResult =
            await controller.GetMatchById(id, CancellationToken.None);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMatchById_WhenFailure_Returns400()
    {
        Mock<IMediator> mediator = new();
        FootballMatchesController controller = new(
            mediator.Object,
            Mock.Of<ILogger<FootballMatchesController>>());

        mediator
            .Setup(m => m.Send(It.IsAny<GetFootballMatchByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FootballMatchDto>.Failure("Match is cancelled."));

        ActionResult<ApiResponse<FootballMatchDto>> actionResult =
            await controller.GetMatchById(Guid.NewGuid(), CancellationToken.None);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
