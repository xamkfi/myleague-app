namespace Application.Features.Common.Organization.Persons.DTOs;

/// <summary>
/// Public player-sports view of a person. Does not include contact or address data.
/// </summary>
public record PersonPlayerSportsDto(
    Guid PersonId,
    string FullName,
    DateTime? BirthDate,
    IReadOnlyList<PersonSportPlayerDto> Sports,
    IReadOnlyList<PersonPlayerLicenceDto> Licences);

/// <summary>
/// A sport-specific player profile belonging to a person.
/// Sport is one of: floorball, football, hockey.
/// </summary>
public record PersonSportPlayerDto(
    string Sport,
    Guid PlayerId);

/// <summary>
/// A current team-roster licence (paid while <see cref="IsActive"/> is true).
/// </summary>
public record PersonPlayerLicenceDto(
    string Sport,
    Guid TeamId,
    string TeamName,
    Guid? CompetitionId,
    string? CompetitionName,
    bool IsActive);
