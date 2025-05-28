using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Hockey;
using Domain.Enums.Floorball;
using Domain.Enums.Hockey;
using Domain.DomainEvents.Common;
using Domain.DomainEvents.Floorball;

namespace DomainTestProject;

/// <summary>
/// Comprehensive test suite for the Club entity
/// </summary>
public class ClubTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateClub()
    {
        // Arrange
        string name = "Test Club";
        string city = "Test City";
        string country = "Test Country";
        DateTime foundingDate = new DateTime(2020, 1, 1);
        Uri websiteUrl = new Uri("https://testclub.com");
        Uri logoUrl = new Uri("https://testclub.com/logo.png");
        string contactEmail = "contact@testclub.com";

        // Act
        Club club = new Club(name, city, country, foundingDate, websiteUrl, logoUrl, contactEmail);

        // Assert
        club.Should().NotBeNull();
        club.Id.Should().NotBeEmpty();
        club.Name.Should().Be(name);
        club.City.Should().Be(city);
        club.Country.Should().Be(country);
        club.FoundingDate.Should().Be(foundingDate);
        club.WebsiteUrl.Should().Be(websiteUrl);
        club.LogoUrl.Should().Be(logoUrl);
        club.ContactEmail.Should().Be(contactEmail);
        club.FloorballTeams.Should().BeEmpty();
        club.HockeyTeams.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMinimalParameters_ShouldCreateClubWithDefaults()
    {
        // Arrange
        string name = "Test Club";
        string city = "Test City";
        string country = "Test Country";

        // Act
        Club club = new Club(name, city, country);

        // Assert
        club.Should().NotBeNull();
        club.Name.Should().Be(name);
        club.City.Should().Be(city);
        club.Country.Should().Be(country);
        club.FoundingDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        club.WebsiteUrl.Should().Be(new Uri("https://example.com"));
        club.LogoUrl.Should().Be(new Uri("https://example.com/logo.png"));
        club.ContactEmail.Should().Be("contact@example.com");
    }

    [Theory]
    [InlineData(null, "City", "Country")]
    [InlineData("", "City", "Country")]
    [InlineData("   ", "City", "Country")]
    [InlineData("Name", null, "Country")]
    [InlineData("Name", "", "Country")]
    [InlineData("Name", "   ", "Country")]
    [InlineData("Name", "City", null)]
    [InlineData("Name", "City", "")]
    [InlineData("Name", "City", "   ")]
    public void Constructor_WithInvalidParameters_ShouldThrowException(string? name, string? city, string? country)
    {
        // Act & Assert
        Action action = () => new Club(name!, city!, country!);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldRaiseClubRegisteredEvent()
    {
        // Arrange
        string name = "Test Club";
        string city = "Test City";
        string country = "Test Country";

        // Act
        Club club = new Club(name, city, country);

        // Assert
        club.DomainEvents.Should().HaveCount(1);
        Domain.DomainEvents.IDomainEvent domainEvent = club.DomainEvents.First();
        domainEvent.Should().BeOfType<ClubRegisteredEvent>();
        
        ClubRegisteredEvent clubRegisteredEvent = (ClubRegisteredEvent)domainEvent;
        clubRegisteredEvent.ClubId.Should().Be(club.Id);
        clubRegisteredEvent.Name.Should().Be(name);
        clubRegisteredEvent.City.Should().Be(city);
        clubRegisteredEvent.Country.Should().Be(country);
        clubRegisteredEvent.FoundingDate.Should().Be(club.FoundingDate);
    }

    #endregion

    #region UpdateBasicInfo Tests

    [Fact]
    public void UpdateBasicInfo_WithValidParameters_ShouldUpdateClub()
    {
        // Arrange
        Club club = new Club("Original Name", "Original City", "Original Country");
        club.ClearDomainEvents(); // Clear the registration event
        
        string newName = "Updated Name";
        string newCity = "Updated City";
        string newCountry = "Updated Country";

        // Act
        club.UpdateBasicInfo(newName, newCity, newCountry);

        // Assert
        club.Name.Should().Be(newName);
        club.City.Should().Be(newCity);
        club.Country.Should().Be(newCountry);
    }

    [Fact]
    public void UpdateBasicInfo_ShouldRaiseClubInfoUpdatedEvent()
    {
        // Arrange
        Club club = new Club("Original Name", "Original City", "Original Country");
        club.ClearDomainEvents();
        
        string newName = "Updated Name";
        string newCity = "Updated City";
        string newCountry = "Updated Country";

        // Act
        club.UpdateBasicInfo(newName, newCity, newCountry);

        // Assert
        club.DomainEvents.Should().HaveCount(1);
        Domain.DomainEvents.IDomainEvent domainEvent = club.DomainEvents.First();
        domainEvent.Should().BeOfType<ClubInfoUpdatedEvent>();
        
        ClubInfoUpdatedEvent clubInfoUpdatedEvent = (ClubInfoUpdatedEvent)domainEvent;
        clubInfoUpdatedEvent.ClubId.Should().Be(club.Id);
        clubInfoUpdatedEvent.Name.Should().Be(newName);
        clubInfoUpdatedEvent.City.Should().Be(newCity);
        clubInfoUpdatedEvent.Country.Should().Be(newCountry);
    }

    [Theory]
    [InlineData(null, "City", "Country")]
    [InlineData("", "City", "Country")]
    [InlineData("   ", "City", "Country")]
    [InlineData("Name", null, "Country")]
    [InlineData("Name", "", "Country")]
    [InlineData("Name", "   ", "Country")]
    [InlineData("Name", "City", null)]
    [InlineData("Name", "City", "")]
    [InlineData("Name", "City", "   ")]
    public void UpdateBasicInfo_WithInvalidParameters_ShouldThrowException(string? name, string? city, string? country)
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");

        // Act & Assert
        Action action = () => club.UpdateBasicInfo(name!, city!, country!);
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region UpdateOnlinePresence Tests

    [Fact]
    public void UpdateOnlinePresence_WithValidParameters_ShouldUpdateClub()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        Uri newWebsiteUrl = new Uri("https://newwebsite.com");
        Uri newLogoUrl = new Uri("https://newwebsite.com/newlogo.png");
        string newContactEmail = "newcontact@testclub.com";

        // Act
        club.UpdateOnlinePresence(newWebsiteUrl, newLogoUrl, newContactEmail);

        // Assert
        club.WebsiteUrl.Should().Be(newWebsiteUrl);
        club.LogoUrl.Should().Be(newLogoUrl);
        club.ContactEmail.Should().Be(newContactEmail);
    }

    [Fact]
    public void UpdateOnlinePresence_WithNullParameters_ShouldUseDefaults()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");

        // Act
        club.UpdateOnlinePresence(null, null, null);

        // Assert
        club.WebsiteUrl.Should().Be(new Uri("https://example.com"));
        club.LogoUrl.Should().Be(new Uri("https://example.com/logo.png"));
        club.ContactEmail.Should().Be("contact@example.com");
    }

    #endregion

    #region UpdateFoundingDate Tests

    [Fact]
    public void UpdateFoundingDate_WithValidDate_ShouldUpdateFoundingDate()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        DateTime newFoundingDate = new DateTime(1990, 5, 15);

        // Act
        club.UpdateFoundingDate(newFoundingDate);

        // Assert
        club.FoundingDate.Should().Be(newFoundingDate);
    }

    #endregion

    #region Floorball Team Management Tests

    [Fact]
    public void AddFloorballTeam_WithValidParameters_ShouldAddTeam()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        string teamName = "Test Floorball Team";
        FloorballDivision division = FloorballDivision.Premier;
        string homeArena = "Test Arena";
        string primaryColor = "Blue";
        string secondaryColor = "White";

        // Act
        FloorballTeam team = club.AddFloorballTeam(teamName, division, homeArena, primaryColor, secondaryColor);

        // Assert
        team.Should().NotBeNull();
        team.Name.Should().Be(teamName);
        team.Division.Should().Be(division);
        team.HomeArena.Should().Be(homeArena);
        team.PrimaryJerseyColor.Should().Be(primaryColor);
        team.SecondaryJerseyColor.Should().Be(secondaryColor);
        club.FloorballTeams.Should().HaveCount(1);
        club.FloorballTeams.Should().Contain(team);
    }

    [Theory]
    [InlineData(null, "Arena", "Blue")]
    [InlineData("", "Arena", "Blue")]
    [InlineData("   ", "Arena", "Blue")]
    [InlineData("Team", null, "Blue")]
    [InlineData("Team", "", "Blue")]
    [InlineData("Team", "   ", "Blue")]
    [InlineData("Team", "Arena", null)]
    [InlineData("Team", "Arena", "")]
    [InlineData("Team", "Arena", "   ")]
    public void AddFloorballTeam_WithInvalidParameters_ShouldThrowException(string? name, string? arena, string? color)
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");

        // Act & Assert
        Action action = () => club.AddFloorballTeam(name!, FloorballDivision.Premier, arena!, color!);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveFloorballTeam_WithExistingTeamWithoutActiveMembers_ShouldRemoveTeam()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        FloorballTeam team = club.AddFloorballTeam("Test Team", FloorballDivision.Premier, "Arena", "Blue");
        club.ClearDomainEvents(); // Clear previous events

        // Act
        bool result = club.RemoveFloorballTeam(team.Id);

        // Assert
        result.Should().BeTrue();
        club.FloorballTeams.Should().BeEmpty();
        club.DomainEvents.Should().HaveCount(1);
        Domain.DomainEvents.IDomainEvent domainEvent = club.DomainEvents.First();
        domainEvent.Should().BeOfType<FloorballTeamRemovedEvent>();
    }

    [Fact]
    public void RemoveFloorballTeam_WithNonExistentTeam_ShouldReturnFalse()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        Guid nonExistentTeamId = Guid.NewGuid();

        // Act
        bool result = club.RemoveFloorballTeam(nonExistentTeamId);

        // Assert
        result.Should().BeFalse();
        club.DomainEvents.Should().HaveCount(1); // Only the registration event
    }

    [Fact]
    public void GetFloorballTeamsByDivision_WithTeamsInDifferentDivisions_ShouldReturnCorrectTeams()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        FloorballTeam premierTeam = club.AddFloorballTeam("Premier Team", FloorballDivision.Premier, "Arena1", "Blue");
        FloorballTeam division1Team = club.AddFloorballTeam("Division1 Team", FloorballDivision.Division1, "Arena2", "Red");
        FloorballTeam anotherPremierTeam = club.AddFloorballTeam("Another Premier Team", FloorballDivision.Premier, "Arena3", "Green");

        // Act
        IEnumerable<FloorballTeam> premierTeams = club.GetFloorballTeamsByDivision(FloorballDivision.Premier);
        IEnumerable<FloorballTeam> division1Teams = club.GetFloorballTeamsByDivision(FloorballDivision.Division1);

        // Assert
        premierTeams.Should().HaveCount(2);
        premierTeams.Should().Contain(premierTeam);
        premierTeams.Should().Contain(anotherPremierTeam);
        
        division1Teams.Should().HaveCount(1);
        division1Teams.Should().Contain(division1Team);
    }

    #endregion

    #region Hockey Team Management Tests

    [Fact]
    public void AddHockeyTeam_WithValidParameters_ShouldAddTeam()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        string teamName = "Test Hockey Team";
        HockeyDivision division = HockeyDivision.Premier;
        string homeArena = "Test Arena";
        string primaryColor = "Red";
        string secondaryColor = "Black";

        // Act
        HockeyTeam team = club.AddHockeyTeam(teamName, division, homeArena, primaryColor, secondaryColor);

        // Assert
        team.Should().NotBeNull();
        team.Name.Should().Be(teamName);
        team.Division.Should().Be(division);
        team.HomeArena.Should().Be(homeArena);
        team.PrimaryJerseyColor.Should().Be(primaryColor);
        team.SecondaryJerseyColor.Should().Be(secondaryColor);
        club.HockeyTeams.Should().HaveCount(1);
        club.HockeyTeams.Should().Contain(team);
    }

    [Theory]
    [InlineData(null, "Arena", "Red")]
    [InlineData("", "Arena", "Red")]
    [InlineData("   ", "Arena", "Red")]
    [InlineData("Team", null, "Red")]
    [InlineData("Team", "", "Red")]
    [InlineData("Team", "   ", "Red")]
    [InlineData("Team", "Arena", null)]
    [InlineData("Team", "Arena", "")]
    [InlineData("Team", "Arena", "   ")]
    public void AddHockeyTeam_WithInvalidParameters_ShouldThrowException(string? name, string? arena, string? color)
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");

        // Act & Assert
        Action action = () => club.AddHockeyTeam(name!, HockeyDivision.Premier, arena!, color!);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveHockeyTeam_WithExistingTeamWithoutActiveMembers_ShouldRemoveTeam()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        HockeyTeam team = club.AddHockeyTeam("Test Team", HockeyDivision.Premier, "Arena", "Red");

        // Act
        bool result = club.RemoveHockeyTeam(team.Id);

        // Assert
        result.Should().BeTrue();
        club.HockeyTeams.Should().BeEmpty();
    }

    [Fact]
    public void RemoveHockeyTeam_WithNonExistentTeam_ShouldReturnFalse()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        Guid nonExistentTeamId = Guid.NewGuid();

        // Act
        bool result = club.RemoveHockeyTeam(nonExistentTeamId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHockeyTeamsByDivision_WithTeamsInDifferentDivisions_ShouldReturnCorrectTeams()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        HockeyTeam premierTeam = club.AddHockeyTeam("Premier Team", HockeyDivision.Premier, "Arena1", "Blue");
        HockeyTeam division1Team = club.AddHockeyTeam("Division1 Team", HockeyDivision.Division1, "Arena2", "Red");
        HockeyTeam anotherPremierTeam = club.AddHockeyTeam("Another Premier Team", HockeyDivision.Premier, "Arena3", "Green");

        // Act
        IEnumerable<HockeyTeam> premierTeams = club.GetHockeyTeamsByDivision(HockeyDivision.Premier);
        IEnumerable<HockeyTeam> division1Teams = club.GetHockeyTeamsByDivision(HockeyDivision.Division1);

        // Assert
        premierTeams.Should().HaveCount(2);
        premierTeams.Should().Contain(premierTeam);
        premierTeams.Should().Contain(anotherPremierTeam);
        
        division1Teams.Should().HaveCount(1);
        division1Teams.Should().Contain(division1Team);
    }

    #endregion

    #region Mixed Team Management Tests

    [Fact]
    public void Club_WithBothFloorballAndHockeyTeams_ShouldManageBothCorrectly()
    {
        // Arrange
        Club club = new Club("Multi-Sport Club", "Test City", "Test Country");

        // Act
        FloorballTeam floorballTeam = club.AddFloorballTeam("FB Team", FloorballDivision.Premier, "Arena1", "Blue");
        HockeyTeam hockeyTeam = club.AddHockeyTeam("Hockey Team", HockeyDivision.Premier, "Arena2", "Red");

        // Assert
        club.FloorballTeams.Should().HaveCount(1);
        club.HockeyTeams.Should().HaveCount(1);
        club.FloorballTeams.Should().Contain(floorballTeam);
        club.HockeyTeams.Should().Contain(hockeyTeam);
    }

    #endregion

    #region Domain Events Tests

    [Fact]
    public void Club_ShouldInheritFromAggregateRoot()
    {
        // Arrange & Act
        Club club = new Club("Test Club", "Test City", "Test Country");

        // Assert
        club.Should().BeAssignableTo<Domain.EventSourcing.AggregateRoot>();
        club.DomainEvents.Should().NotBeNull();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        club.UpdateBasicInfo("Updated Name", "Updated City", "Updated Country");
        club.DomainEvents.Should().HaveCount(2); // Registration + Update events

        // Act
        club.ClearDomainEvents();

        // Assert
        club.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void Club_WithMultipleOperations_ShouldMaintainConsistentState()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");

        // Act - Perform multiple operations
        club.UpdateBasicInfo("Updated Club", "Updated City", "Updated Country");
        club.UpdateFoundingDate(new DateTime(1995, 3, 10));
        club.UpdateOnlinePresence(
            new Uri("https://updated.com"), 
            new Uri("https://updated.com/logo.png"), 
            "updated@club.com");

        FloorballTeam fbTeam1 = club.AddFloorballTeam("FB Team 1", FloorballDivision.Premier, "Arena1", "Blue");
        FloorballTeam fbTeam2 = club.AddFloorballTeam("FB Team 2", FloorballDivision.Division1, "Arena2", "Red");
        HockeyTeam hockeyTeam1 = club.AddHockeyTeam("Hockey Team 1", HockeyDivision.Premier, "Arena3", "Green");

        // Assert - Verify final state
        club.Name.Should().Be("Updated Club");
        club.City.Should().Be("Updated City");
        club.Country.Should().Be("Updated Country");
        club.FoundingDate.Should().Be(new DateTime(1995, 3, 10));
        club.WebsiteUrl.Should().Be(new Uri("https://updated.com"));
        club.LogoUrl.Should().Be(new Uri("https://updated.com/logo.png"));
        club.ContactEmail.Should().Be("updated@club.com");

        club.FloorballTeams.Should().HaveCount(2);
        club.HockeyTeams.Should().HaveCount(1);

        // Verify teams are correctly associated
        club.GetFloorballTeamsByDivision(FloorballDivision.Premier).Should().Contain(fbTeam1);
        club.GetFloorballTeamsByDivision(FloorballDivision.Division1).Should().Contain(fbTeam2);
        club.GetHockeyTeamsByDivision(HockeyDivision.Premier).Should().Contain(hockeyTeam1);
    }

    [Fact]
    public void Club_WithSpecialCharactersInName_ShouldHandleCorrectly()
    {
        // Arrange & Act
        Club club = new Club("Åkersberga IK", "Åkersberga", "Sverige");

        // Assert
        club.Name.Should().Be("Åkersberga IK");
        club.City.Should().Be("Åkersberga");
        club.Country.Should().Be("Sverige");
    }

    [Fact]
    public void Club_WithVeryLongValidStrings_ShouldHandleCorrectly()
    {
        // Arrange
        string longName = new string('A', 100);
        string longCity = new string('B', 100);
        string longCountry = new string('C', 100);

        // Act
        Club club = new Club(longName, longCity, longCountry);

        // Assert
        club.Name.Should().Be(longName);
        club.City.Should().Be(longCity);
        club.Country.Should().Be(longCountry);
    }

    #endregion
} 
