using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Teams.DTOs;

/// <summary>
/// The team's reusable loan goalkeeper, ready to be selected for a match.
/// </summary>
public record FloorballLoanGoalkeeperDto(
    Guid PlayerId,
    Guid RosterEntryId,
    string FirstName,
    string LastName,
    string DisplayName,
    FloorballPosition Position,
    bool IsLoanGoalkeeper);
