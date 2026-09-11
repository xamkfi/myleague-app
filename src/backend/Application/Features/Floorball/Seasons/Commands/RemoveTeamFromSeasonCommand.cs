using MediatR;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;

namespace Application.Features.Floorball.Seasons.Commands;

public record RemoveTeamFromSeasonCommand(Guid CompetitionId, Guid TeamId) : IRequest<Result<FloorballSeasonDto>>; 
