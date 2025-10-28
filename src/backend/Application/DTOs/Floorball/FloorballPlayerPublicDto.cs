

using Application.DTOs.Common;
using Domain.Enums.Floorball;

namespace Application.DTOs.Floorball
{
    public record FloorballPlayerPublicDto(
        Guid Id,
        Guid PersonId,
        PersonPublicDto Person,
        bool IsActive,
        FloorballPosition Position,
        int CareerGoals,
        int CareerAssists,
        FloorballTeamNameDto? Team = null);
}
