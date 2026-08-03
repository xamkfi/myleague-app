using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

public record CreateHockeyTeamCommand(
    string Name,
    Guid ClubId,
    TeamCategory TeamCategory,
    Guid? DivisionId = null,
    string? HomeArena = null,
    string? PrimaryJerseyColor = null,
    string? SecondaryJerseyColor = null,
    string? ShortName = null) : IRequest<Result<HockeyTeamDto>>;
