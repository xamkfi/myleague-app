using Application.Common;
using Application.DTOs.Common;
using Application.Handlers.Clubs;
using Application.Queries.Clubs;
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

namespace ApplicationTestProject.Handlers.Clubs;

public class GetAllClubsHandlerTests
{
    private readonly Mock<IClubRepository> _mockClubRepository;
    private readonly Mock<ILogger<GetAllClubsHandler>> _mockLogger;
    private readonly GetAllClubsHandler _handler;

    public GetAllClubsHandlerTests()
    {
        _mockClubRepository = new Mock<IClubRepository>();
        _mockLogger = new Mock<ILogger<GetAllClubsHandler>>();
        _handler = new GetAllClubsHandler(_mockClubRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsSuccessResult()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

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

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(clubs);

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(3);

        List<ClubDto> clubDtos = result.Data!.ToList();
        clubDtos[0].Name.Should().Be("Club 1");
        clubDtos[0].WebsiteUrl.Should().Be("https://club1.com/");
        clubDtos[1].Name.Should().Be("Club 2");
        clubDtos[1].WebsiteUrl.Should().Be("https://club2.com/");
        clubDtos[2].Name.Should().Be("Club 3");
        clubDtos[2].WebsiteUrl.Should().Be("https://example.com/");
        clubDtos[2].LogoUrl.Should().Be("https://example.com/logo.png");
        clubDtos[2].ContactEmail.Should().Be("contact@example.com");

        _mockClubRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyCollection()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

        List<Club> emptyClubs = new List<Club>();

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(emptyClubs);

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();

        _mockClubRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SingleClub_ReturnsSuccessResult()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

        List<Club> clubs = new List<Club>
        {
            new Club(
                "Single Club",
                "Single City",
                "Single Country",
                new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            )
        };

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(clubs);

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);

        ClubDto clubDto = result.Data!.First();
        clubDto.Name.Should().Be("Single Club");
        clubDto.City.Should().Be("Single City");
        clubDto.Country.Should().Be("Single Country");

        _mockClubRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while retrieving");
        result.Data.Should().BeNull();

        _mockClubRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_LogsAppropriateMessages_OnSuccess()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

        List<Club> clubs = new List<Club>
        {
            new Club("Club 1", "City 1", "Country 1", new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            new Club("Club 2", "City 2", "Country 2", new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        };

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(clubs);

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logging calls were made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving all clubs")),
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
        GetAllClubsQuery query = new GetAllClubsQuery();

        Exception testException = new Exception("Test exception");
        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ThrowsAsync(testException);

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        // Verify error log was made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while retrieving all clubs")),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LargeNumberOfClubs_ReturnsSuccessResult()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

        List<Club> clubs = new List<Club>();
        for (int i = 1; i <= 100; i++)
        {
            clubs.Add(new Club(
                $"Club {i}",
                $"City {i}",
                $"Country {i}",
                new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(i)
            ));
        }

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(clubs);

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(100);

        List<ClubDto> clubDtos = result.Data!.ToList();
        clubDtos[0].Name.Should().Be("Club 1");
        clubDtos[99].Name.Should().Be("Club 100");

        _mockClubRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ClubsWithMixedData_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

        List<Club> clubs = new List<Club>
        {
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
        };

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(clubs);

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(3);

        List<ClubDto> clubDtos = result.Data!.ToList();

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

        _mockClubRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleCallsToSameQuery_EachCallProcessedIndependently()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

        List<Club> clubs = new List<Club>
        {
            new Club("Test Club", "Test City", "Test Country", new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        };

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(clubs);

        // Act
        Result<IEnumerable<ClubDto>> result1 = await _handler.Handle(query, CancellationToken.None);
        Result<IEnumerable<ClubDto>> result2 = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Data!.Should().HaveCount(1);
        result2.Data!.Should().HaveCount(1);

        _mockClubRepository.Verify(x => x.GetAllAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_NullReturnFromRepository_ReturnsFailureResult()
    {
        // Arrange
        GetAllClubsQuery query = new GetAllClubsQuery();

        _mockClubRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync((IEnumerable<Club>?)null);

        // Act
        Result<IEnumerable<ClubDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNullOrEmpty();
    }
} 
