using System.Security.Claims;
using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Constants;
using Domain.Enums.Hockey.Matches;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebAPI.Controllers.Hockey;
using WebAPI.Models.Hockey;

namespace WebApiTestProject.Controllers.Hockey;

public class HockeyMatchLifecycleControllerTests
{
    [Theory]
    [InlineData(HockeyMatchStatus.Cancelled)]
    [InlineData(HockeyMatchStatus.Scheduled)]
    [InlineData(HockeyMatchStatus.Finished)]
    public async Task SetStatus_MatchOfficialOtherThanReopen_ReturnsForbidden(HockeyMatchStatus status)
    {
        Mock<IMediator> mediator = new();
        HockeyMatchLifecycleController controller = CreateController(mediator, AuthRoles.MatchOfficial);

        ActionResult<WebAPI.Models.Common.ApiResponse<HockeyMatchDto>> actionResult = await controller.SetStatus(
            Guid.NewGuid(), new SetHockeyMatchStatusRequest { Status = status }, CancellationToken.None);

        actionResult.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        mediator.Verify(m => m.Send(It.IsAny<SetHockeyMatchStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(AuthRoles.MatchOfficial, HockeyMatchStatus.InProgress)]
    [InlineData(AuthRoles.SystemAdmin, HockeyMatchStatus.Cancelled)]
    public async Task SetStatus_AllowedRoleAndStatus_SendsCommand(string role, HockeyMatchStatus status)
    {
        Mock<IMediator> mediator = new();
        mediator
            .Setup(m => m.Send(It.IsAny<SetHockeyMatchStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HockeyMatchDto>.Failure("not under test"));
        HockeyMatchLifecycleController controller = CreateController(mediator, role);
        Guid matchId = Guid.NewGuid();

        await controller.SetStatus(matchId, new SetHockeyMatchStatusRequest { Status = status }, CancellationToken.None);

        mediator.Verify(
            m => m.Send(
                It.Is<SetHockeyMatchStatusCommand>(c => c.MatchId == matchId && c.Status == status),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static HockeyMatchLifecycleController CreateController(Mock<IMediator> mediator, string role)
    {
        ClaimsPrincipal user = new(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test"));
        return new HockeyMatchLifecycleController(mediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                    RequestServices = new ServiceCollection().BuildServiceProvider(),
                },
            },
        };
    }
}
