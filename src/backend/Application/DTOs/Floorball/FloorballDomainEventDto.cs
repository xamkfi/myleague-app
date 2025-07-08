using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Floorball
{
    public record FloorballDomainEventDto(
        string EventType,
        DateTime OccurredOn,
        object Data);

}
