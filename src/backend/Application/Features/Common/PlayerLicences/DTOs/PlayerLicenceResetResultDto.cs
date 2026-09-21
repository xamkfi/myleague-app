namespace Application.Features.Common.PlayerLicences.DTOs;

public record PlayerLicenceResetResultDto(
    bool Ran,
    int CutoffYear,
    int FloorballDeactivated,
    int FootballDeactivated,
    int HockeyDeactivated);
