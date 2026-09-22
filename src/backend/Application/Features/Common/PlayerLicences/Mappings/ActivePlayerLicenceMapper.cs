using Application.Features.Common.PlayerLicences.DTOs;
using Domain.Common;

namespace Application.Features.Common.PlayerLicences.Mappings;

public static class ActivePlayerLicenceMapper
{
    public static IReadOnlyList<ActivePlayerLicenceDto> ToDtos(IReadOnlyList<PlayerLicenceRow>? rows)
    {
        if (rows is null || rows.Count == 0)
        {
            return Array.Empty<ActivePlayerLicenceDto>();
        }

        List<ActivePlayerLicenceDto> licences = new List<ActivePlayerLicenceDto>(rows.Count);
        foreach (PlayerLicenceRow row in rows)
        {
            licences.Add(new ActivePlayerLicenceDto(
                row.TeamId,
                row.TeamName,
                row.CompetitionId,
                row.CompetitionName));
        }

        return licences;
    }
}
