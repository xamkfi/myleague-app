using Application.Commands.Clubs;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ApplicationTestProject.Mappings;

public class ClubMapperTests
{
    [Fact]
    public void ToDto_ValidClubWithFullData_ReturnsCorrectDto()
    {
        // Arrange
        Club club = new Club(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new Uri("https://testclub.com"),
            new Uri("https://testclub.com/logo.png"),
            "contact@testclub.com"
        );

        // Act
        ClubDto result = ClubMapper.ToDto(club);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(club.Id);
        result.Name.Should().Be(club.Name);
        result.City.Should().Be(club.City);
        result.Country.Should().Be(club.Country);
        result.FoundingDate.Should().Be(club.FoundingDate);
        result.WebsiteUrl.Should().Be(club.WebsiteUrl!.ToString());
        result.LogoUrl.Should().Be(club.LogoUrl!.ToString());
        result.ContactEmail.Should().Be(club.ContactEmail);
    }

    [Fact]
    public void ToDto_ValidClubWithMinimalData_ReturnsCorrectDto()
    {
        // Arrange
        Club club = new Club(
            "Minimal Club",
            "City",
            "Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        // Act
        ClubDto result = ClubMapper.ToDto(club);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(club.Id);
        result.Name.Should().Be(club.Name);
        result.City.Should().Be(club.City);
        result.Country.Should().Be(club.Country);
        result.FoundingDate.Should().Be(club.FoundingDate);
        result.WebsiteUrl.Should().Be("https://example.com/");
        result.LogoUrl.Should().Be("https://example.com/logo.png");
        result.ContactEmail.Should().Be("contact@example.com");
    }

    [Fact]
    public void ToDto_ClubWithNullUrls_ReturnsEmptyStringsInDto()
    {
        // Arrange
        Club club = new Club(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            null,
            null,
            ""
        );

        // Act
        ClubDto result = ClubMapper.ToDto(club);

        // Assert
        result.Should().NotBeNull();
        result.WebsiteUrl.Should().Be("https://example.com/");
        result.LogoUrl.Should().Be("https://example.com/logo.png");
        result.ContactEmail.Should().Be("");
    }

    [Fact]
    public void ToDto_NullClub_ThrowsArgumentNullException()
    {
        // Arrange
        Club? club = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ClubMapper.ToDto(club!));
    }

    [Fact]
    public void ToDtos_ValidClubCollection_ReturnsCorrectDtoCollection()
    {
        // Arrange
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
                new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ),
            new Club(
                "Club 3",
                "City 3",
                "Country 3",
                new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new Uri("https://club3.com"),
                null,
                "contact@club3.com"
            )
        };

        // Act
        IEnumerable<ClubDto> result = ClubMapper.ToDtos(clubs);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        List<ClubDto> dtoList = result.ToList();

        // First club with full data
        dtoList[0].Name.Should().Be("Club 1");
        dtoList[0].WebsiteUrl.Should().Be("https://club1.com/");
        dtoList[0].LogoUrl.Should().Be("https://club1.com/logo.png");
        dtoList[0].ContactEmail.Should().Be("contact@club1.com");

        // Second club with minimal data
        dtoList[1].Name.Should().Be("Club 2");
        dtoList[1].WebsiteUrl.Should().Be("https://example.com/");
        dtoList[1].LogoUrl.Should().Be("https://example.com/logo.png");
        dtoList[1].ContactEmail.Should().Be("contact@example.com");

        // Third club with partial data
        dtoList[2].Name.Should().Be("Club 3");
        dtoList[2].WebsiteUrl.Should().Be("https://club3.com/");
        dtoList[2].LogoUrl.Should().Be("https://example.com/logo.png");
        dtoList[2].ContactEmail.Should().Be("contact@club3.com");
    }

    [Fact]
    public void ToDtos_EmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        List<Club> clubs = new List<Club>();

        // Act
        IEnumerable<ClubDto> result = ClubMapper.ToDtos(clubs);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToDtos_NullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<Club>? clubs = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ClubMapper.ToDtos(clubs!));
    }

    [Fact]
    public void ToEntity_ValidCreateCommand_ReturnsCorrectEntity()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "New Club",
            "New City",
            "New Country",
            new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "https://newclub.com",
            "https://newclub.com/logo.png",
            "contact@newclub.com"
        );

        // Act
        Club result = ClubMapper.ToEntity(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.City.Should().Be(command.City);
        result.Country.Should().Be(command.Country);
        result.FoundingDate.Should().Be(command.FoundingDate);
        result.WebsiteUrl.Should().NotBeNull();
        result.WebsiteUrl!.ToString().Should().Be("https://newclub.com/");
        result.LogoUrl.Should().NotBeNull();
        result.LogoUrl!.ToString().Should().Be(command.LogoUrl);
        result.ContactEmail.Should().Be(command.ContactEmail);
    }

    [Fact]
    public void ToEntity_CreateCommandWithMinimalData_ReturnsCorrectEntity()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Minimal Club",
            "City",
            "Country",
            new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        // Act
        Club result = ClubMapper.ToEntity(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.City.Should().Be(command.City);
        result.Country.Should().Be(command.Country);
        result.FoundingDate.Should().Be(command.FoundingDate);
        result.WebsiteUrl.Should().Be(new Uri("https://example.com/"));
        result.LogoUrl.Should().Be(new Uri("https://example.com/logo.png"));
        result.ContactEmail.Should().Be("");
    }

    [Fact]
    public void ToEntity_CreateCommandWithEmptyUrls_ReturnsEntityWithDefaultUrls()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "",
            "",
            ""
        );

        // Act
        Club result = ClubMapper.ToEntity(command);

        // Assert
        result.Should().NotBeNull();
        result.WebsiteUrl.Should().Be(new Uri("https://example.com/"));
        result.LogoUrl.Should().Be(new Uri("https://example.com/logo.png"));
        result.ContactEmail.Should().Be("");
    }

    [Fact]
    public void ToEntity_NullCreateCommand_ThrowsArgumentNullException()
    {
        // Arrange
        CreateClubCommand? command = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ClubMapper.ToEntity(command!));
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ToEntity_CreateCommandWithNonUtcDate_ConvertsToUtc(DateTimeKind dateTimeKind)
    {
        // Arrange
        DateTime foundingDate = dateTimeKind == DateTimeKind.Local 
            ? new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Local)
            : new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);

        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            foundingDate
        );

        // Act
        Club result = ClubMapper.ToEntity(command);

        // Assert
        result.Should().NotBeNull();
        result.FoundingDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void UpdateFromCommand_ValidUpdateCommand_UpdatesEntityCorrectly()
    {
        // Arrange
        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new Uri("https://original.com"),
            new Uri("https://original.com/logo.png"),
            "original@club.com"
        );

        UpdateClubCommand command = new UpdateClubCommand(
            existingClub.Id,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "https://updated.com",
            "https://updated.com/logo.png",
            "updated@club.com"
        );

        // Act
        ClubMapper.UpdateFromCommand(existingClub, command);

        // Assert
        existingClub.Name.Should().Be(command.Name);
        existingClub.City.Should().Be(command.City);
        existingClub.Country.Should().Be(command.Country);
        existingClub.FoundingDate.Should().Be(command.FoundingDate);
        existingClub.WebsiteUrl.Should().NotBeNull();
        existingClub.WebsiteUrl!.ToString().Should().Be("https://updated.com/");
        existingClub.LogoUrl.Should().NotBeNull();
        existingClub.LogoUrl!.ToString().Should().Be(command.LogoUrl);
        existingClub.ContactEmail.Should().Be(command.ContactEmail);
    }

    [Fact]
    public void UpdateFromCommand_UpdateCommandWithEmptyUrls_UpdatesEntityWithDefaultUrls()
    {
        // Arrange
        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new Uri("https://original.com"),
            new Uri("https://original.com/logo.png"),
            "original@club.com"
        );

        UpdateClubCommand command = new UpdateClubCommand(
            existingClub.Id,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "",
            "",
            ""
        );

        // Act
        ClubMapper.UpdateFromCommand(existingClub, command);

        // Assert
        existingClub.WebsiteUrl.Should().Be(new Uri("https://example.com/"));
        existingClub.LogoUrl.Should().Be(new Uri("https://example.com/logo.png"));
        existingClub.ContactEmail.Should().Be("");
    }

    [Fact]
    public void UpdateFromCommand_NullClub_ThrowsArgumentNullException()
    {
        // Arrange
        Club? club = null;
        UpdateClubCommand command = new UpdateClubCommand(
            Guid.NewGuid(),
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ClubMapper.UpdateFromCommand(club!, command));
    }

    [Fact]
    public void UpdateFromCommand_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        Club club = new Club(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );
        UpdateClubCommand? command = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ClubMapper.UpdateFromCommand(club, command!));
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void UpdateFromCommand_UpdateCommandWithNonUtcDate_ConvertsToUtc(DateTimeKind dateTimeKind)
    {
        // Arrange
        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        DateTime foundingDate = dateTimeKind == DateTimeKind.Local 
            ? new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Local)
            : new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);

        UpdateClubCommand command = new UpdateClubCommand(
            existingClub.Id,
            "Updated Club",
            "Updated City",
            "Updated Country",
            foundingDate
        );

        // Act
        ClubMapper.UpdateFromCommand(existingClub, command);

        // Assert
        existingClub.FoundingDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void UpdateFromCommand_PreservesClubId_DoesNotChangeId()
    {
        // Arrange
        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );
        Guid originalId = existingClub.Id;

        UpdateClubCommand command = new UpdateClubCommand(
            Guid.NewGuid(),
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        // Act
        ClubMapper.UpdateFromCommand(existingClub, command);

        // Assert
        existingClub.Id.Should().Be(originalId);
        existingClub.Id.Should().NotBe(command.ClubId);
    }

    [Theory]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("http://test.org/path", "http://test.org/path")]
    [InlineData("https://subdomain.domain.com:8080/path?query=value", "https://subdomain.domain.com:8080/path?query=value")]
    public void ToEntity_ValidUrls_ParsesCorrectly(string inputUrl, string expectedUrl)
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            inputUrl,
            inputUrl
        );

        // Act
        Club result = ClubMapper.ToEntity(command);

        // Assert
        result.WebsiteUrl.Should().NotBeNull();
        result.WebsiteUrl!.ToString().Should().Be(expectedUrl);
        result.LogoUrl.Should().NotBeNull();
        result.LogoUrl!.ToString().Should().Be(expectedUrl);
    }
} 