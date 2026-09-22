using MediatR;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;

namespace Application.Features.Floorball.Competitions.Commands;

public record RemoveFloorballTeamFromSeasonCommand(Guid CompetitionId, Guid TeamId) : IRequest<Result<FloorballSeasonDto>>; 
