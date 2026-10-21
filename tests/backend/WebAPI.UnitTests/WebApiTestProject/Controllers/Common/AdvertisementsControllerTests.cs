using Application.Common;
using Application.Features.Common.Content.Advertisements.Commands;
using Application.Features.Common.Content.Advertisements.DTOs;
using Domain.Enums.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;

namespace WebApiTestProject.Controllers.Common;

public class AdvertisementsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private AdvertisementsController Controller() =>
        new(_mediator.Object, Mock.Of<ILogger<AdvertisementsController>>());

    private static AdvertisementRequest Request() => new()
    {
        Name = "Sponsor",
        ImageUrl = "https://cdn.example.com/ad.png",
        Slot = AdSlot.LeftTop,
        IsActive = true,
    };

    [Fact]
    public async Task Create_WhenSuccessful_Returns201()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateAdvertisementCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AdvertisementDto>.Success(new AdvertisementDto { Id = Guid.NewGuid(), Name = "Sponsor" }));

        ActionResult<ApiResponse<AdvertisementDto>> actionResult = await Controller().Create(Request());

        ObjectResult result = actionResult.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Update_WhenNotFound_Returns404()
    {
        Guid id = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<UpdateAdvertisementCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AdvertisementDto>.NotFound("Advertisement", id));

        ActionResult<ApiResponse<AdvertisementDto>> actionResult = await Controller().Update(id, Request());

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UploadImage_WithWrongContentType_Returns400WithoutSending()
    {
        using MemoryStream stream = new([1, 2, 3]);
        FormFile file = new(stream, 0, stream.Length, "file", "ad.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf",
        };

        ActionResult<ApiResponse<string>> actionResult = await Controller().UploadImage(file);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        _mediator.VerifyNoOtherCalls();
    }
}
