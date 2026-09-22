using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Queries;
using Domain.Common;
using Domain.Enums.Common;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.Hockey;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Hockey;

namespace WebApiTestProject.Controllers.Hockey;

public class HockeyMatchControllerTests
{
    [Fact]
    public async Task GetList_MapsQueryAndReturnsOk()
    {
        Mock<IMediator> mediator = new();
        HockeyMatchController controller = new(mediator.Object);

        DateTime start = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);
        HockeyMatchListDto item = new(
            Guid.NewGuid(),
            start.AddDays(3),
            "Scheduled",
            "League",
            "Arena",
            Guid.NewGuid(),
            "Liiga",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "HIFK",
            "Tappara",
            0,
            0);

        mediator
            .Setup(m => m.Send(It.IsAny<GetHockeyMatchesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<HockeyMatchListDto>>.Success(
                PagedResult.Create(new List<HockeyMatchListDto> { item }, 1, 1, 100)));

        ActionResult<PaginatedApiResponse<HockeyMatchListDto>> actionResult = await controller.GetList(
            new GetHockeyMatchesRequest
            {
                Page = 1,
                PageSize = 100,
                StartDate = start,
                EndDate = end,
                TeamCategory = TeamCategory.Adult,
                SortOrder = "asc",
            },
            CancellationToken.None);

        actionResult.Result.Should().BeOfType<OkObjectResult>();
        mediator.Verify(
            m => m.Send(
                It.Is<GetHockeyMatchesQuery>(query =>
                    query.Page == 1
                    && query.PageSize == 100
                    && query.StartDate == start
                    && query.EndDate == end
                    && query.TeamCategory == TeamCategory.Adult
                    && query.SortOrder == "asc"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
