namespace Application.Features.Common.Persons.DTOs;

/// <summary>
/// Public player-sports view of a person. Does not include contact or address data.
/// </summary>
public record PersonPlayerSportsDto(
    Guid PersonId,
    string FullName,
    DateTime? BirthDate,
    IReadOnlyList<PersonSportPlayerDto> Sports);

/// <summary>
/// A sport-specific player profile belonging to a person.
/// Sport is one of: floorball, football, hockey.
/// </summary>
public record PersonSportPlayerDto(
    string Sport,
    Guid PlayerId);
