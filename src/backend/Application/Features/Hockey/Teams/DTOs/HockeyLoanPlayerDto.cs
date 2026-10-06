using Domain.Enums.Hockey.Teams;

namespace Application.Features.Hockey.Teams.DTOs;

/// <summary>
/// One reusable loan player, ready to be selected for a match.
/// <see cref="RosterEntryId"/> is the roster membership id used by the match lineup.
/// </summary>
public record HockeyLoanPlayerDto(
    Guid PlayerId,
    Guid RosterEntryId,
    string FirstName,
    string LastName,
    string DisplayName,
    int JerseyNumber,
    int LoanPlayerNumber,
    HockeyPosition Position,
    bool IsLoanPlayer);
