namespace Application.Features.Common.Persons.DTOs;

/// <summary>
/// DTO representing a person with their teams
/// </summary>
public record PersonWithTeamsDto(
    int Id,
    string Name,
    int Age,
    List<PersonTeamDto> teams,
    int totalMatchesPlayed
);

/// <summary>
/// DTO representing a team from a person's perspective
/// </summary>
public record PersonTeamDto(
    int Id,
    string Name,
    PlayerInfoDto Players
);

/// <summary>
/// DTO representing player info within a team
/// </summary>
public record PlayerInfoDto(
    List<PersonPlayerDto> Goalkeepers,
    List<PersonPlayerDto> Fieldplayers
);

/// <summary>
/// DTO representing a person as a player in a team
/// </summary>
public record PersonPlayerDto(
    int Id,
    int? Number,
    int MatchesPlayed,
    int? GoalsScored = null,
    int? Assists = null
); 
