//using Domain.Entities.Common;
//using Domain.Entities.Floorball;
//using Domain.Entities.Hockey;
//using Domain.Enums.Common;
//using Domain.Enums.Floorball;
//using Domain.Enums.Hockey;

//namespace DomainTestProject;

///// <summary>
///// Test suite focused on edge cases and error scenarios for the Club entity
///// </summary>
//public class ClubEdgeCasesTests
//{
//    #region Boundary Value Tests

//    [Fact]
//    public void Constructor_WithMinimumValidStrings_ShouldSucceed()
//    {
//        // Arrange
//        string name = "A";
//        string city = "B";
//        string country = "C";

//        // Act
//        Club club = new Club(name, city, country);

//        // Assert
//        club.Name.Should().Be(name);
//        club.City.Should().Be(city);
//        club.Country.Should().Be(country);
//    }

//    [Fact]
//    public void Constructor_WithMaximumReasonableStrings_ShouldSucceed()
//    {
//        // Arrange
//        string name = new string('A', 500);
//        string city = new string('B', 500);
//        string country = new string('C', 500);

//        // Act
//        Club club = new Club(name, city, country);

//        // Assert
//        club.Name.Should().Be(name);
//        club.City.Should().Be(city);
//        club.Country.Should().Be(country);
//    }

//    [Fact]
//    public void Constructor_WithUnicodeCharacters_ShouldHandleCorrectly()
//    {
//        // Arrange
//        string name = "Футбольный клуб Зенит";
//        string city = "Санкт-Петербург";
//        string country = "Россия";

//        // Act
//        Club club = new Club(name, city, country);

//        // Assert
//        club.Name.Should().Be(name);
//        club.City.Should().Be(city);
//        club.Country.Should().Be(country);
//    }

//    [Fact]
//    public void Constructor_WithSpecialCharacters_ShouldHandleCorrectly()
//    {
//        // Arrange
//        string name = "FC Barcelona & Real Madrid C.F.";
//        string city = "Barcelona/Madrid";
//        string country = "España";

//        // Act
//        Club club = new Club(name, city, country);

//        // Assert
//        club.Name.Should().Be(name);
//        club.City.Should().Be(city);
//        club.Country.Should().Be(country);
//    }

//    #endregion

//    #region Date Boundary Tests

//    [Fact]
//    public void Constructor_WithMinDateTime_ShouldSucceed()
//    {
//        // Arrange
//        DateTime foundingDate = DateTime.MinValue;

//        // Act
//        Club club = new Club("Test Club", "Test City", "Test Country", foundingDate);

//        // Assert
//        club.FoundingDate.Should().Be(foundingDate);
//    }

//    [Fact]
//    public void Constructor_WithMaxDateTime_ShouldSucceed()
//    {
//        // Arrange
//        DateTime foundingDate = DateTime.MaxValue;

//        // Act
//        Club club = new Club("Test Club", "Test City", "Test Country", foundingDate);

//        // Assert
//        club.FoundingDate.Should().Be(foundingDate);
//    }

//    [Fact]
//    public void UpdateFoundingDate_WithFutureDate_ShouldSucceed()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        DateTime futureDate = DateTime.UtcNow.AddYears(100);

//        // Act
//        club.UpdateFoundingDate(futureDate);

//        // Assert
//        club.FoundingDate.Should().Be(futureDate);
//    }

//    [Fact]
//    public void UpdateFoundingDate_WithVeryOldDate_ShouldSucceed()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        DateTime oldDate = new DateTime(1800, 1, 1);

//        // Act
//        club.UpdateFoundingDate(oldDate);

//        // Assert
//        club.FoundingDate.Should().Be(oldDate);
//    }

//    #endregion

//    #region URI Edge Cases

//    [Fact]
//    public void Constructor_WithComplexUris_ShouldHandleCorrectly()
//    {
//        // Arrange
//        Uri websiteUrl = new Uri("https://sub.domain.example.com:8080/path/to/page?param=value&other=123#section");
//        Uri logoUrl = new Uri("https://cdn.example.com/images/logos/club-logo-2023-v2.png");

//        // Act
//        Club club = new Club("Test Club", "Test City", "Test Country", null, websiteUrl, logoUrl);

//        // Assert
//        club.WebsiteUrl.Should().Be(websiteUrl);
//        club.LogoUrl.Should().Be(logoUrl);
//    }

//    [Fact]
//    public void UpdateOnlinePresence_WithFileUris_ShouldHandleCorrectly()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        Uri fileWebsiteUrl = new Uri("file:///C:/websites/club/index.html");
//        Uri fileLogoUrl = new Uri("file:///C:/images/logo.png");

//        // Act
//        club.UpdateOnlinePresence(fileWebsiteUrl, fileLogoUrl, "test@example.com");

//        // Assert
//        club.WebsiteUrl.Should().Be(fileWebsiteUrl);
//        club.LogoUrl.Should().Be(fileLogoUrl);
//    }

//    #endregion

//    #region Team Management Edge Cases

//    [Fact]
//    public void AddFloorballTeam_WithAllDivisionTypes_ShouldSucceed()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");

//        // Act & Assert
//        foreach (FloorballDivision division in Enum.GetValues<FloorballDivision>())
//        {
//            FloorballTeam team = club.AddFloorballTeam($"Team {division}", division, "Arena", "Color", TeamCategory.Adult);
//            team.Division.Should().Be(division);
//        }

//        club.FloorballTeams.Should().HaveCount(Enum.GetValues<FloorballDivision>().Length);
//    }

//    [Fact]
//    public void AddHockeyTeam_WithAllDivisionTypes_ShouldSucceed()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");

//        // Act & Assert
//        foreach (HockeyDivision division in Enum.GetValues<HockeyDivision>())
//        {
//            HockeyTeam team = club.AddHockeyTeam($"Team {division}", division, "Arena", "Color");
//            team.Division.Should().Be(division);
//        }

//        club.HockeyTeams.Should().HaveCount(Enum.GetValues<HockeyDivision>().Length);
//    }

//    [Fact]
//    public void AddTeams_WithSameNameDifferentSports_ShouldSucceed()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        string teamName = "Duplicate Name Team";

//        // Act
//        FloorballTeam floorballTeam = club.AddFloorballTeam(teamName, FloorballDivision.Premier, "Arena1", "Blue", TeamCategory.Adult);
//        HockeyTeam hockeyTeam = club.AddHockeyTeam(teamName, HockeyDivision.Premier, "Arena2", "Red");

//        // Assert
//        floorballTeam.Name.Should().Be(teamName);
//        hockeyTeam.Name.Should().Be(teamName);
//        club.FloorballTeams.Should().Contain(floorballTeam);
//        club.HockeyTeams.Should().Contain(hockeyTeam);
//    }

//    [Fact]
//    public void AddManyTeams_ShouldMaintainCorrectCollections()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        int teamCount = 50;

//        // Act
//        for (int i = 0; i < teamCount; i++)
//        {
//            club.AddFloorballTeam($"FB Team {i}", FloorballDivision.Premier, $"Arena {i}", $"Color {i}", TeamCategory.Adult);
//            club.AddHockeyTeam($"Hockey Team {i}", HockeyDivision.Premier, $"Arena {i}", $"Color {i}");
//        }

//        // Assert
//        club.FloorballTeams.Should().HaveCount(teamCount);
//        club.HockeyTeams.Should().HaveCount(teamCount);
        
//        // Verify all teams have unique names
//        club.FloorballTeams.Select(t => t.Name).Should().OnlyHaveUniqueItems();
//        club.HockeyTeams.Select(t => t.Name).Should().OnlyHaveUniqueItems();
//    }

//    [Fact]
//    public void RemoveFloorballTeam_MultipleTimesWithSameId_ShouldReturnFalseAfterFirst()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        FloorballTeam team = club.AddFloorballTeam("Test Team", FloorballDivision.Premier, "Arena", "Blue", TeamCategory.Adult);
//        Guid teamId = team.Id;

//        // Act
//        bool firstRemoval = club.RemoveFloorballTeam(teamId);
//        bool secondRemoval = club.RemoveFloorballTeam(teamId);
//        bool thirdRemoval = club.RemoveFloorballTeam(teamId);

//        // Assert
//        firstRemoval.Should().BeTrue();
//        secondRemoval.Should().BeFalse();
//        thirdRemoval.Should().BeFalse();
//        club.FloorballTeams.Should().BeEmpty();
//    }

//    [Fact]
//    public void RemoveHockeyTeam_MultipleTimesWithSameId_ShouldReturnFalseAfterFirst()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        HockeyTeam team = club.AddHockeyTeam("Test Team", HockeyDivision.Premier, "Arena", "Red");
//        Guid teamId = team.Id;

//        // Act
//        bool firstRemoval = club.RemoveHockeyTeam(teamId);
//        bool secondRemoval = club.RemoveHockeyTeam(teamId);
//        bool thirdRemoval = club.RemoveHockeyTeam(teamId);

//        // Assert
//        firstRemoval.Should().BeTrue();
//        secondRemoval.Should().BeFalse();
//        thirdRemoval.Should().BeFalse();
//        club.HockeyTeams.Should().BeEmpty();
//    }

//    #endregion

//    #region Division Filtering Edge Cases

//    [Fact]
//    public void GetFloorballTeamsByDivision_WithNoneDivision_ShouldReturnCorrectTeams()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        FloorballTeam noneTeam = club.AddFloorballTeam("None Team", FloorballDivision.None, "Arena", "Color", TeamCategory.Adult);
//        club.AddFloorballTeam("Premier Team", FloorballDivision.Premier, "Arena", "Color", TeamCategory.Adult);

//        // Act
//        IEnumerable<FloorballTeam> noneTeams = club.GetFloorballTeamsByDivision(FloorballDivision.None);

//        // Assert
//        noneTeams.Should().HaveCount(1);
//        noneTeams.Should().Contain(noneTeam);
//    }

//    [Fact]
//    public void GetHockeyTeamsByDivision_WithNoneDivision_ShouldReturnCorrectTeams()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        HockeyTeam noneTeam = club.AddHockeyTeam("None Team", HockeyDivision.None, "Arena", "Color");
//        club.AddHockeyTeam("Premier Team", HockeyDivision.Premier, "Arena", "Color");

//        // Act
//        IEnumerable<HockeyTeam> noneTeams = club.GetHockeyTeamsByDivision(HockeyDivision.None);

//        // Assert
//        noneTeams.Should().HaveCount(1);
//        noneTeams.Should().Contain(noneTeam);
//    }

//    [Fact]
//    public void GetTeamsByDivision_WithNonExistentDivision_ShouldReturnEmpty()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        club.AddFloorballTeam("Team 1", FloorballDivision.Premier, "Arena", "Color", TeamCategory.Adult);
//        club.AddHockeyTeam("Team 2", HockeyDivision.Division1, "Arena", "Color");

//        // Act
//        IEnumerable<FloorballTeam> floorballTeams = club.GetFloorballTeamsByDivision(FloorballDivision.Division4);
//        IEnumerable<HockeyTeam> hockeyTeams = club.GetHockeyTeamsByDivision(HockeyDivision.Veterans);

//        // Assert
//        floorballTeams.Should().BeEmpty();
//        hockeyTeams.Should().BeEmpty();
//    }

//    #endregion

//    #region Concurrent Operations Simulation

//    [Fact]
//    public void MultipleOperations_InSequence_ShouldMaintainConsistency()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");

//        // Act - Simulate multiple operations
//        for (int i = 0; i < 10; i++)
//        {
//            club.UpdateBasicInfo($"Club {i}", $"City {i}", $"Country {i}");
//            club.UpdateFoundingDate(new DateTime(2000 + i, 1, 1));
            
//            FloorballTeam fbTeam = club.AddFloorballTeam($"FB Team {i}", FloorballDivision.Premier, $"Arena {i}", "Blue", TeamCategory.Adult);
//            HockeyTeam hockeyTeam = club.AddHockeyTeam($"Hockey Team {i}", HockeyDivision.Premier, $"Arena {i}", "Red");
            
//            if (i % 2 == 0)
//            {
//                club.RemoveFloorballTeam(fbTeam.Id);
//                club.RemoveHockeyTeam(hockeyTeam.Id);
//            }
//        }

//        // Assert
//        club.Name.Should().Be("Club 9");
//        club.City.Should().Be("City 9");
//        club.Country.Should().Be("Country 9");
//        club.FoundingDate.Should().Be(new DateTime(2009, 1, 1));
        
//        // Should have 5 teams of each type (odd indices)
//        club.FloorballTeams.Should().HaveCount(5);
//        club.HockeyTeams.Should().HaveCount(5);
//    }

//    #endregion

//    #region Memory and Performance Edge Cases

//    [Fact]
//    public void Club_WithManyDomainEvents_ShouldHandleCorrectly()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");

//        // Act - Generate many domain events
//        for (int i = 0; i < 100; i++)
//        {
//            club.UpdateBasicInfo($"Club {i}", $"City {i}", $"Country {i}");
//        }

//        // Assert
//        club.DomainEvents.Should().HaveCount(101); // 1 registration + 100 updates
//        club.Name.Should().Be("Club 99");
        
//        // Verify all events have unique IDs
//        club.DomainEvents.Select(e => e.Id).Should().OnlyHaveUniqueItems();
//    }

//    [Fact]
//    public void Club_AfterClearingEvents_ShouldContinueWorkingNormally()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");
//        club.UpdateBasicInfo("Updated", "Updated", "Updated");
//        club.AddFloorballTeam("Team", FloorballDivision.Premier, "Arena", "Color", TeamCategory.Adult);

//        // Act
//        club.ClearDomainEvents();
//        club.UpdateBasicInfo("Final", "Final", "Final");
//        FloorballTeam newTeam = club.AddFloorballTeam("New Team", FloorballDivision.Division1, "New Arena", "New Color", TeamCategory.Adult);

//        // Assert
//        club.DomainEvents.Should().HaveCount(1); // Only the final update
//        club.Name.Should().Be("Final");
//        club.FloorballTeams.Should().HaveCount(2);
//        club.FloorballTeams.Should().Contain(newTeam);
//    }

//    #endregion

//    #region Null and Empty Collection Handling

//    [Fact]
//    public void Club_NewInstance_ShouldHaveEmptyCollections()
//    {
//        // Arrange & Act
//        Club club = new Club("Test Club", "Test City", "Test Country");

//        // Assert
//        club.FloorballTeams.Should().NotBeNull();
//        club.FloorballTeams.Should().BeEmpty();
//        club.HockeyTeams.Should().NotBeNull();
//        club.HockeyTeams.Should().BeEmpty();
//        club.DomainEvents.Should().NotBeNull();
//        club.DomainEvents.Should().HaveCount(1); // Registration event
//    }

//    [Fact]
//    public void Club_Collections_ShouldBeReadOnly()
//    {
//        // Arrange
//        Club club = new Club("Test Club", "Test City", "Test Country");

//        // Act & Assert
//        club.FloorballTeams.Should().BeAssignableTo<IReadOnlyList<FloorballTeam>>();
//        club.HockeyTeams.Should().BeAssignableTo<IReadOnlyList<HockeyTeam>>();
//        club.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<Domain.DomainEvents.IDomainEvent>>();
//    }

//    #endregion
//} 
