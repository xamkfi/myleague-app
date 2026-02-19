using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums.Floorball;
using Application.DTOs.Common;
using Domain.ValueObjects.Floorball;
using Domain.Entities.Common;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Data Transfer Object for FloorballPlayer entity
    /// </summary>
    public record FloorballPlayerDto(
        Guid Id,
        Guid PersonId,
        PersonDto Person,
        bool IsActive,
        FloorballPosition Position,
        int CareerGoals,
        int CareerAssists,
        FloorballTeamNameDto? Team = null);
}
