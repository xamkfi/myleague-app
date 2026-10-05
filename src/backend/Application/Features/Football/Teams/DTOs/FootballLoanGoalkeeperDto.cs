using Domain.Enums.Football;

namespace Application.Features.Football.Teams.DTOs;

/// <summary>
/// The team's reusable loan goalkeeper, ready to be selected for a match.
/// </summary>
public record FootballLoanGoalkeeperDto(
    Guid PlayerId,
    Guid RosterEntryId,
    string FirstName,
    string LastName,
    string DisplayName,
    FootballPosition Position,
    bool IsLoanGoalkeeper);
