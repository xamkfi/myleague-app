using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Queries;

/// <summary>
/// Gets a hockey match by id.
/// </summary>
public record GetHockeyMatchByIdQuery(Guid MatchId) : IRequest<Result<HockeyMatchDto>>;
