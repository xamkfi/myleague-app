using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command: UpdateHockeyTeam.
/// </summary>
public record UpdateHockeyTeamCommand(
    Guid TeamId,
    string Name,
    string? ShortName,
    TeamCategory TeamCategory,
    Guid? DivisionId,
    string? HomeArena,
    string? PrimaryJerseyColor,
    string? SecondaryJerseyColor) : IRequest<Result<HockeyTeamDto>>;
