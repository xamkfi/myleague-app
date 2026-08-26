using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Queries;
using Domain.Enums.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Controllers.Football;
using WebAPI.Models.Common;

namespace WebApiTestProject.Controllers.Football;

public class FootballSeasonControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly FootballSeasonController _controller;

    public FootballSeasonControllerTests()
    {
        _controller = new FootballSeasonController(
            _mediator.Object,
            Mock.Of<ILogger<FootballSeasonController>>());
    }

    private static FootballSeasonDto CreateSeasonDto(Guid id) =>
        new(
            id,
            "Football 2026",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive: false,
            IsCompleted: false,
            SeasonDivisions: Array.Empty<FootballSeasonDivisionDto>(),
            Teams: Array.Empty<FootballTeamSummaryDto>(),
            Matches: Array.Empty<FootballMatchDto>(),
            MatchRules: new FootballMatchRulesDto(2, 20, 5, true, 0, false, false, 2, 5, false),
            StandingRules: new FootballStandingRulesDto(3, 1, 0),
            TeamCategory: TeamCategory.Adult);

    [Fact]
    public async Task GetSeasonById_WhenFound_ReturnsOk()
    {
        Guid id = Guid.NewGuid();
        FootballSeasonDto dto = CreateSeasonDto(id);
        _mediator
            .Setup(m => m.Send(It.Is<GetFootballSeasonByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FootballSeasonDto>.Success(dto));

        ActionResult<ApiResponse<FootballSeasonDto>> actionResult = await _controller.GetSeasonById(id);

        OkObjectResult ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<FootballSeasonDto> body = ok.Value.Should().BeOfType<ApiResponse<FootballSeasonDto>>().Subject;
        body.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetSeasonById_WhenNotFound_Returns404()
    {
        Guid id = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<GetFootballSeasonByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FootballSeasonDto>.NotFound("FootballSeason", id));

        ActionResult<ApiResponse<FootballSeasonDto>> actionResult = await _controller.GetSeasonById(id);

        NotFoundObjectResult notFound = actionResult.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        ApiResponse<FootballSeasonDto> body = notFound.Value.Should().BeOfType<ApiResponse<FootballSeasonDto>>().Subject;
        body.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetSeasonById_WhenFailure_Returns400()
    {
        Guid id = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<GetFootballSeasonByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FootballSeasonDto>.Failure("Cannot complete season."));

        ActionResult<ApiResponse<FootballSeasonDto>> actionResult = await _controller.GetSeasonById(id);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
