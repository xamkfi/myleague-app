using Application.Common;
using Application.Features.Common.Organization.PlayerLicences.Commands;
using Application.Features.Common.Organization.PlayerLicences.DTOs;
using Application.Features.Common.SiteSettings.Commands;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Features.Common.SiteSettings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;

namespace WebApiTestProject.Controllers.Common;

public class SiteSettingsControllerTests
{
    [Fact]
    public async Task Get_WhenSuccessful_ReturnsOk()
    {
        Mock<IMediator> mediator = new();
        SiteSettingsController controller = new(
            mediator.Object,
            Mock.Of<ILogger<SiteSettingsController>>());

        SiteSettingsDto dto = new(15, 7, 10, 5, 5, false);
        mediator
            .Setup(m => m.Send(It.IsAny<GetSiteSettingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SiteSettingsDto>.Success(dto));

        ActionResult<ApiResponse<SiteSettingsDto>> actionResult = await controller.Get();

        OkObjectResult ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<SiteSettingsDto> body = ok.Value.Should().BeOfType<ApiResponse<SiteSettingsDto>>().Subject;
        body.Success.Should().BeTrue();
        body.Data!.AccessTokenExpirationMinutes.Should().Be(15);
        body.Data.IsPersisted.Should().BeFalse();
    }

    [Fact]
    public async Task Update_WhenSuccessful_SendsCommandAndReturnsOk()
    {
        Mock<IMediator> mediator = new();
        SiteSettingsController controller = new(
            mediator.Object,
            Mock.Of<ILogger<SiteSettingsController>>());

        SiteSettingsDto dto = new(20, 14, 8, 6, 4, true);
        mediator
            .Setup(m => m.Send(It.IsAny<UpdateSiteSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SiteSettingsDto>.Success(dto));

        ActionResult<ApiResponse<SiteSettingsDto>> actionResult = await controller.Update(
            new UpdateSiteSettingsRequest
            {
                AccessTokenExpirationMinutes = 20,
                RefreshTokenExpirationDays = 14,
                LoginCodeExpirationMinutes = 8,
                LoginCodeMaxAttempts = 6,
                SessionExpiryWarningMinutes = 4
            });

        OkObjectResult ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<SiteSettingsDto> body = ok.Value.Should().BeOfType<ApiResponse<SiteSettingsDto>>().Subject;
        body.Success.Should().BeTrue();
        body.Data!.IsPersisted.Should().BeTrue();
        body.Data.AccessTokenExpirationMinutes.Should().Be(20);

        mediator.Verify(
            m => m.Send(
                It.Is<UpdateSiteSettingsCommand>(c =>
                    c.AccessTokenExpirationMinutes == 20
                    && c.RefreshTokenExpirationDays == 14
                    && c.LoginCodeExpirationMinutes == 8
                    && c.LoginCodeMaxAttempts == 6
                    && c.SessionExpiryWarningMinutes == 4
                    && c.PlayerLicenceResetMonth == 5
                    && c.PlayerLicenceResetDay == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPlayerLicences_WhenSuccessful_SendsForceCommand()
    {
        Mock<IMediator> mediator = new();
        SiteSettingsController controller = new(
            mediator.Object,
            Mock.Of<ILogger<SiteSettingsController>>());

        PlayerLicenceResetResultDto dto = new(true, 2026, 3, 2, 1);
        mediator
            .Setup(m => m.Send(It.IsAny<ResetExpiredPlayerLicencesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PlayerLicenceResetResultDto>.Success(dto));

        ActionResult<ApiResponse<PlayerLicenceResetResultDto>> actionResult =
            await controller.ResetPlayerLicences();

        OkObjectResult ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<PlayerLicenceResetResultDto> body =
            ok.Value.Should().BeOfType<ApiResponse<PlayerLicenceResetResultDto>>().Subject;
        body.Success.Should().BeTrue();
        body.Data!.Ran.Should().BeTrue();

        mediator.Verify(
            m => m.Send(
                It.Is<ResetExpiredPlayerLicencesCommand>(c => c.Force),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
