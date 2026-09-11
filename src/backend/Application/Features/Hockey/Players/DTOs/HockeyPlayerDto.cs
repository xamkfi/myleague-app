namespace Application.Features.Hockey.Players.DTOs;

/// <summary>
/// Data transfer object for a hockey player profile.
/// </summary>
public record HockeyPlayerDto(
    Guid Id,
    Guid PersonId,
    string? LicenseNumber,
    bool IsActive,
    string PrimaryPosition,
    string Shoots,
    string? Catches,
    int CareerGamesPlayed,
    int CareerGoals,
    int CareerAssists,
    int CareerPenaltyMinutes);
