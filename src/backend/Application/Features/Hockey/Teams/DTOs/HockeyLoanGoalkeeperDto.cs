using Domain.Enums.Hockey.Teams;

namespace Application.Features.Hockey.Teams.DTOs;

/// <summary>
/// The team's reusable loan goalkeeper, ready to be selected for a match.
/// <see cref="RosterEntryId"/> is the roster membership id used by the match lineup.
/// </summary>
public record HockeyLoanGoalkeeperDto(
    Guid PlayerId,
    Guid RosterEntryId,
    string FirstName,
    string LastName,
    string DisplayName,
    HockeyPosition Position,
    bool IsLoanGoalkeeper);
