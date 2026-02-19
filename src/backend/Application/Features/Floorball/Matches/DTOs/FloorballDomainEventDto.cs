using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Matches.DTOs
{
    public record FloorballDomainEventDto(
        string EventType,
        DateTime OccurredOn,
        object Data);

}
