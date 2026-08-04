using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Assigns an official to a hockey match.
/// </summary>
public record AddHockeyMatchOfficialCommand(
    Guid MatchId,
    Guid OfficialId,
    HockeyOfficialRole Role,
    bool IsMainOfficial = false) : IRequest<Result<HockeyMatchDto>>;
