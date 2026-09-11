namespace Application.Features.Football.Seasons.DTOs;

/// <summary>
/// Slim team projection used on season payloads.
/// </summary>
public record FootballTeamSummaryDto(
    Guid Id,
    string Name,
    string ShortName,
    Guid ClubId,
    Uri? LogoUrl);
