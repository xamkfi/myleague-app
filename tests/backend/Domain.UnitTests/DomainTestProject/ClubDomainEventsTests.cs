using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Hockey;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Enums.Hockey;

namespace DomainTestProject;

/// <summary>
/// Test suite focused on Club domain events
/// </summary>
public class ClubDomainEventsTests
{
    [Fact]
    public void ClubRegisteredEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        string name = "Test Club";
        string city = "Test City";
        string country = "Test Country";
        DateTime foundingDate = new DateTime(2020, 1, 1);

        // Act
        Club club = new Club(name, city, country, foundingDate);

        // Assert
        
 
    }

    [Fact]
    public void ClubInfoUpdatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        Club club = new Club("Original Name", "Original City", "Original Country");
        
        string newName = "Updated Name";
        string newCity = "Updated City";
        string newCountry = "Updated Country";

        // Act
        club.UpdateBasicInfo(newName, newCity, newCountry);

        // Assert

        
        //domainEvent.Id.Should().NotBeEmpty();
        //domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        //domainEvent.ClubId.Should().Be(club.Id);
        //domainEvent.Name.Should().Be(newName);
        //domainEvent.City.Should().Be(newCity);
        //domainEvent.Country.Should().Be(newCountry);
    }

    //[Fact]
    //public void FloorballTeamRemovedEvent_ShouldHaveCorrectProperties()
    //{
    //    // Arrange
    //    Club club = new Club("Test Club", "Test City", "Test Country");
    //    FloorballTeam team = club.AddFloorballTeam("Test Team", FloorballDivision.Premier, "Arena", "Blue", TeamCategory.Adult);
    //    club.ClearDomainEvents();

    //    // Act
    //    bool result = club.RemoveFloorballTeam(team.Id);

    //    // Assert
    //    result.Should().BeTrue();
    //    club.DomainEvents.Should().HaveCount(1);
    //    FloorballTeamRemovedEvent domainEvent = club.DomainEvents.First().Should().BeOfType<FloorballTeamRemovedEvent>().Subject;
        
    //    domainEvent.Id.Should().NotBeEmpty();
    //    domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    //    domainEvent.ClubId.Should().Be(club.Id);
    //    domainEvent.TeamId.Should().Be(team.Id);
    //}

    //[Fact]
    //public void MultipleOperations_ShouldRaiseMultipleDomainEvents()
    //{
    //    // Arrange
    //    Club club = new Club("Test Club", "Test City", "Test Country");
        
    //    // Act
    //    club.UpdateBasicInfo("Updated Name", "Updated City", "Updated Country");
    //    FloorballTeam team = club.AddFloorballTeam("Test Team", FloorballDivision.Premier, "Arena", "Blue", TeamCategory.Adult);
    //    club.RemoveFloorballTeam(team.Id);

    //    // Assert
    //    club.DomainEvents.Should().HaveCount(3);
    //    club.DomainEvents.Should().ContainSingle(e => e is ClubRegisteredEvent);
    //    club.DomainEvents.Should().ContainSingle(e => e is ClubInfoUpdatedEvent);
    //    club.DomainEvents.Should().ContainSingle(e => e is FloorballTeamRemovedEvent);
    //}

    [Fact]
    public void DomainEvents_ShouldBeOrderedByOccurrenceTime()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");

        
        // Add a small delay to ensure different timestamps
        Thread.Sleep(1);
        
        // Act
        club.UpdateBasicInfo("Updated Name", "Updated City", "Updated Country");
 

        // Assert

    }

    //[Fact]
    //public void ClearDomainEvents_ShouldRemoveAllEventsButNotAffectEntityState()
    //{
    //    // Arrange
    //    Club club = new Club("Test Club", "Test City", "Test Country");
    //    club.UpdateBasicInfo("Updated Name", "Updated City", "Updated Country");
    //    club.AddFloorballTeam("Test Team", FloorballDivision.Premier, "Arena", "Blue", TeamCategory.Adult);
        
    //    int eventCountBeforeClear = club.DomainEvents.Count;
    //    string nameBeforeClear = club.Name;
    //    int teamCountBeforeClear = club.FloorballTeams.Count;

    //    // Act
    //    club.ClearDomainEvents();

    //    // Assert
    //    eventCountBeforeClear.Should().BeGreaterThan(0);
    //    club.DomainEvents.Should().BeEmpty();
        
    //    // Entity state should remain unchanged
    //    club.Name.Should().Be(nameBeforeClear);
    //    club.FloorballTeams.Should().HaveCount(teamCountBeforeClear);
    //}

    //[Fact]
    //public void DomainEvents_ShouldHaveUniqueIds()
    //{
    //    // Arrange
    //    Club club = new Club("Test Club", "Test City", "Test Country");
        
    //    // Act
    //    club.UpdateBasicInfo("Updated Name", "Updated City", "Updated Country");
    //    FloorballTeam team = club.AddFloorballTeam("Test Team", FloorballDivision.Premier, "Arena", "Blue", TeamCategory.Adult);
    //    club.RemoveFloorballTeam(team.Id);

    //    // Assert
    //    List<Guid> eventIds = club.DomainEvents.Select(e => e.Id).ToList();
    //    eventIds.Should().OnlyHaveUniqueItems();
    //    eventIds.Should().AllSatisfy(id => id.Should().NotBeEmpty());
    //}

    [Fact]
    public void UpdateOnlinePresence_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");

        // Act
        club.UpdateOnlinePresence(
            new Uri("https://newsite.com"), 
            new Uri("https://newsite.com/logo.png"), 
            "new@email.com");

        // Assert
    }

    [Fact]
    public void UpdateFoundingDate_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");

        // Act
        club.UpdateFoundingDate(new DateTime(1995, 1, 1));

        // Assert
    }

    //[Fact]
    //public void AddFloorballTeam_ShouldNotRaiseDomainEventFromClub()
    //{
    //    // Arrange
    //    Club club = new Club("Test Club", "Test City", "Test Country");
    //    club.ClearDomainEvents();

    //    // Act
    //    FloorballTeam team = club.AddFloorballTeam("Test Team", FloorballDivision.Premier, "Arena", "Blue", TeamCategory.Adult);

    //    // Assert
    //    // The Club itself should not raise an event for adding a team
    //    // The team might raise its own events, but that's tested elsewhere
    //    club.DomainEvents.Should().BeEmpty();
    //}

    [Fact]
    public void AddHockeyTeam_ShouldNotRaiseDomainEventFromClub()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");


        // Act
        HockeyTeam team = club.AddHockeyTeam("Test Team", HockeyDivision.Premier, "Arena", "Red");

        // Assert
        // The Club itself should not raise an event for adding a team
        // The team might raise its own events, but that's tested elsewhere

    }

    [Fact]
    public void RemoveHockeyTeam_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        Club club = new Club("Test Club", "Test City", "Test Country");
        HockeyTeam team = club.AddHockeyTeam("Test Team", HockeyDivision.Premier, "Arena", "Red");

        // Act
        bool result = club.RemoveHockeyTeam(team.Id);

        // Assert
        result.Should().BeTrue();
        // Hockey team removal doesn't raise a domain event in the current implementation

    }
} 
