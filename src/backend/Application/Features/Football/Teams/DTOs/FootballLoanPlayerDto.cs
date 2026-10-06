using Domain.Enums.Football;

namespace Application.Features.Football.Teams.DTOs;

/// <summary>
/// One reusable loan player, ready to be selected for a match.
/// </summary>
public record FootballLoanPlayerDto(
    Guid PlayerId,
    Guid RosterEntryId,
    string FirstName,
    string LastName,
    string DisplayName,
    int JerseyNumber,
    int LoanPlayerNumber,
    FootballPosition Position,
    bool IsLoanPlayer);
