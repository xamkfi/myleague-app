using Application.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Moves a hockey season to another audience group (adult, youth, women). Allowed in any season status.
/// </summary>
/// <param name="SeasonId">Season to update.</param>
/// <param name="TeamCategory">New audience group.</param>
public record ChangeHockeySeasonTeamCategoryCommand(Guid SeasonId, TeamCategory TeamCategory) : IRequest<Result>;
