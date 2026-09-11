using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Queries;

/// <summary>
/// Query to retrieve a hockey competition by id.
/// </summary>
public record GetHockeyCompetitionByIdQuery(Guid Id) : IRequest<Result<HockeyCompetitionDto>>;
