namespace Application.Features.Common.Organization.PlayerLicences.DTOs;

public record PlayerLicenceResetResultDto(
    bool Ran,
    int CutoffYear,
    int FloorballDeactivated,
    int FootballDeactivated,
    int HockeyDeactivated);
