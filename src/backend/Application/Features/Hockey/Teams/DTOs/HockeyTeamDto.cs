namespace Application.Features.Hockey.Teams.DTOs;

public record HockeyTeamDto(
    Guid Id,
    string Name,
    string ShortName,
    Guid ClubId,
    Guid? DivisionId,
    string TeamCategory,
    string HomeArena,
    string PrimaryJerseyColor,
    string SecondaryJerseyColor,
    string? LogoUrl,
    bool IsActive);
