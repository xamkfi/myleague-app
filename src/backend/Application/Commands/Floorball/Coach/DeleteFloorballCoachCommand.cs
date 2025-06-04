using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Coach
{
    /// <summary>
    /// Command for deleting a floorball coach
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteFloorballCoachCommand(Guid Id) : IRequest<Result<FloorballCoachDto>>;
} 