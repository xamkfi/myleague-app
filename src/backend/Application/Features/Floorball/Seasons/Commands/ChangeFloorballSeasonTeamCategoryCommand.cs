using Application.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands;

/// <summary>
/// Moves a floorball season to another audience group (adult, youth, women). Allowed in any season status.
/// </summary>
/// <param name="SeasonId">Season to update.</param>
/// <param name="TeamCategory">New audience group.</param>
public record ChangeFloorballSeasonTeamCategoryCommand(Guid SeasonId, TeamCategory TeamCategory) : IRequest<Result>;
