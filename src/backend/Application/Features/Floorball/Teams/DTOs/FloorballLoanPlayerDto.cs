using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Teams.DTOs;

/// <summary>
/// One reusable loan player, ready to be selected for a match.
/// </summary>
public record FloorballLoanPlayerDto(
    Guid PlayerId,
    Guid RosterEntryId,
    string FirstName,
    string LastName,
    string DisplayName,
    int JerseyNumber,
    int LoanPlayerNumber,
    FloorballPosition Position,
    bool IsLoanPlayer);
