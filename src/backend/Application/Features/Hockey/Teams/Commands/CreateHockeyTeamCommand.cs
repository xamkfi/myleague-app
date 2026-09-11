using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command for creating a hockey team.
/// </summary>
/// <param name="Name">Team name</param>
/// <param name="ClubId">Owning club id</param>
/// <param name="TeamCategory">Adult, youth, or women</param>
/// <param name="DivisionId">Optional division id</param>
/// <param name="HomeArena">Optional home arena</param>
/// <param name="PrimaryJerseyColor">Optional primary jersey color</param>
/// <param name="SecondaryJerseyColor">Optional secondary jersey color</param>
/// <param name="ShortName">Optional short name</param>
public record CreateHockeyTeamCommand(
    string Name,
    Guid ClubId,
    TeamCategory TeamCategory,
    Guid? DivisionId = null,
    string? HomeArena = null,
    string? PrimaryJerseyColor = null,
    string? SecondaryJerseyColor = null,
    string? ShortName = null) : IRequest<Result<HockeyTeamDto>>;
