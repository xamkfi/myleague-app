using Application.Common;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Clubs.Handlers;
using Application.Features.Common.Clubs.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ApplicationTestProject.Handlers.Clubs;

public class GetAllClubsHandlerTests
{
    private readonly Mock<IClubRepository> _mockClubRepository;
    private readonly Mock<ILogger<GetAllClubsHandler>> _mockLogger;
    private readonly Mock<IPaginationService> _mockPaginationService;
    private readonly GetAllClubsHandler _handler;

    public GetAllClubsHandlerTests()
    {
        _mockClubRepository = new Mock<IClubRepository>();
        _mockLogger = new Mock<ILogger<GetAllClubsHandler>>();
        _mockPaginationService = new Mock<IPaginationService>();
        
        // Setup default pagination service behavior
        _mockPaginationService.Setup(x => x.ResolvePageSize(It.IsAny<string>(), It.IsAny<int>()))
            .Returns<string, int>((key, size) => size == 0 ? 50 : size);
        _mockPaginationService.Setup(x => x.IsValidPageSize(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(true);
        _mockPaginationService.Setup(x => x.GetPaginationSettings(It.IsAny<string>()))
            .Returns(new PaginationSettings(50, 100, 1));
        
        _handler = new GetAllClubsHandler(
            _mockClubRepository.Object, 
            _mockLogger.Object,
            _mockPaginationService.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsSuccessResult()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 1, PageSize: 50);

        List<Club> clubs = new List<Club>
        {
            new Club(
                "Club 1",
                "City 1",
                "Country 1",
                new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new Uri("https://club1.com"),
                new Uri("https://club1.com/logo.png"),
                "contact@club1.com"
            ),
            new Club(
                "Club 2",
                "City 2",
                "Country 2",
                new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new Uri("https://club2.com"),
                new Uri("https://club2.com/logo.png"),
                "contact@club2.com"
            ),
            new Club(
                "Club 3",
                "City 3",
                "Country 3",
                new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            )
        };

        PagedResult<Club> pagedClubs = PagedResult.Create(clubs, 3, 1, 50);

        _mockClubRepository.Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedClubs);

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);
        result.Data!.TotalCount.Should().Be(3);
        result.Data!.Page.Should().Be(1);
        result.Data!.PageSize.Should().Be(50);

        List<ClubDto> clubDtos = result.Data!.Items.ToList();
        clubDtos[0].Name.Should().Be("Club 1");
        clubDtos[0].WebsiteUrl.Should().Be("https://club1.com/");
        clubDtos[1].Name.Should().Be("Club 2");
        clubDtos[1].WebsiteUrl.Should().Be("https://club2.com/");
        clubDtos[2].Name.Should().Be("Club 3");
        clubDtos[2].WebsiteUrl.Should().Be("https://example.com/");
        clubDtos[2].LogoUrl.Should().Be("https://example.com/logo.png");
        clubDtos[2].ContactEmail.Should().Be("contact@example.com");

        _mockClubRepository.Verify(x => x.GetPagedAsync(1, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyCollection()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 1, PageSize: 50);

        List<Club> emptyClubs = new List<Club>();
        PagedResult<Club> pagedClubs = PagedResult.Create(emptyClubs, 0, 1, 50);

        _mockClubRepository.Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedClubs);

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data!.TotalCount.Should().Be(0);

        _mockClubRepository.Verify(x => x.GetPagedAsync(1, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SingleClub_ReturnsSuccessResult()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 1, PageSize: 50);

        List<Club> clubs = new List<Club>
        {
            new Club(
                "Single Club",
                "Single City",
                "Single Country",
                new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            )
        };

        PagedResult<Club> pagedClubs = PagedResult.Create(clubs, 1, 1, 50);

        _mockClubRepository.Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedClubs);

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);

        ClubDto clubDto = result.Data!.Items.First();
        clubDto.Name.Should().Be("Single Club");
        clubDto.City.Should().Be("Single City");
        clubDto.Country.Should().Be("Single Country");

        _mockClubRepository.Verify(x => x.GetPagedAsync(1, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 1, PageSize: 50);

        _mockClubRepository.Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while retrieving");
        result.Data.Should().BeNull();

        _mockClubRepository.Verify(x => x.GetPagedAsync(1, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LogsAppropriateMessages_OnSuccess()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 1, PageSize: 50);

        List<Club> clubs = new List<Club>
        {
            new Club("Club 1", "City 1", "Country 1", new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            new Club("Club 2", "City 2", "Country 2", new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        };

        PagedResult<Club> pagedClubs = PagedResult.Create(clubs, 2, 1, 50);

        _mockClubRepository.Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedClubs);

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logging calls were made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving clubs")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved") && v.ToString()!.Contains("clubs")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsErrorMessage_OnException()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 1, PageSize: 50);

        Exception testException = new Exception("Test exception");
        _mockClubRepository.Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException);

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        // Verify error log was made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while retrieving clubs")),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PaginationWithMultiplePages_ReturnsCorrectPage()
    {
        // Arrange - Page 2 of 4, pageSize 25, total 100 clubs
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 2, PageSize: 25);

        List<Club> clubs = new List<Club>();
        for (int i = 26; i <= 50; i++)
        {
            clubs.Add(new Club(
                $"Club {i}",
                $"City {i}",
                $"Country {i}",
                new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(i)
            ));
        }

        PagedResult<Club> pagedClubs = PagedResult.Create(clubs, 100, 2, 25);

        _mockClubRepository.Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedClubs);

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(25);
        result.Data!.TotalCount.Should().Be(100);
        result.Data!.Page.Should().Be(2);
        result.Data!.PageSize.Should().Be(25);
        result.Data!.TotalPages.Should().Be(4);
        result.Data!.HasNextPage.Should().BeTrue();
        result.Data!.HasPreviousPage.Should().BeTrue();

        List<ClubDto> clubDtos = result.Data!.Items.ToList();
        clubDtos[0].Name.Should().Be("Club 26");
        clubDtos[24].Name.Should().Be("Club 50");

        _mockClubRepository.Verify(x => x.GetPagedAsync(2, 25, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClubsWithMixedData_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 1, PageSize: 50);

        List<Club> clubs =
        [
            // Club with full data
            new Club(
                "Full Data Club",
                "Full City",
                "Full Country",
                new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new Uri("https://fullclub.com"),
                new Uri("https://fullclub.com/logo.png"),
                "contact@fullclub.com"
            ),
            // Club with minimal data
            new Club(
                "Minimal Club",
                "Min City",
                "Min Country",
                new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ),
            // Club with partial data
            new Club(
                "Partial Club",
                "Partial City",
                "Partial Country",
                new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new Uri("https://partialclub.com"),
                null,
                "partial@club.com"
            )
        ];

        PagedResult<Club> pagedClubs = PagedResult.Create(clubs, 3, 1, 50);

        _mockClubRepository.Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedClubs);

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);

        List<ClubDto> clubDtos = [.. result.Data!.Items];

        // Full data club
        clubDtos[0].Name.Should().Be("Full Data Club");
        clubDtos[0].WebsiteUrl.Should().Be("https://fullclub.com/");
        clubDtos[0].LogoUrl.Should().Be("https://fullclub.com/logo.png");
        clubDtos[0].ContactEmail.Should().Be("contact@fullclub.com");

        // Minimal data club
        clubDtos[1].Name.Should().Be("Minimal Club");
        clubDtos[1].WebsiteUrl.Should().Be("https://example.com/");
        clubDtos[1].LogoUrl.Should().Be("https://example.com/logo.png");
        clubDtos[1].ContactEmail.Should().Be("contact@example.com");

        // Partial data club
        clubDtos[2].Name.Should().Be("Partial Club");
        clubDtos[2].WebsiteUrl.Should().Be("https://partialclub.com/");
        clubDtos[2].LogoUrl.Should().Be("https://example.com/logo.png");
        clubDtos[2].ContactEmail.Should().Be("partial@club.com");

        _mockClubRepository.Verify(x => x.GetPagedAsync(1, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidPageNumber_ReturnsFailure()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 0, PageSize: 50);

        _mockPaginationService.Setup(x => x.IsValidPageSize(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(true);

        // Act
        Result<PagedResult<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Page must be greater than 0");
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery(Page: 1, PageSize: 50);
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _handler.Handle(query, cts.Token));
    }
}
