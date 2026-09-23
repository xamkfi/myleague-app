using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Competitions.Commands;

public record RemoveFootballTeamFromSeasonCommand(Guid CompetitionId, Guid TeamId) : IRequest<Result<FootballSeasonDto>>;
