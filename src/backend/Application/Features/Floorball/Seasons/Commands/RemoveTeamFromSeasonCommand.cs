using MediatR;
using Application.Common;
using Application.DTOs.Floorball;

namespace Application.Commands.Floorball.Season;

public record RemoveTeamFromSeasonCommand(Guid SeasonId, Guid TeamId) : IRequest<Result<FloorballSeasonDto>>; 