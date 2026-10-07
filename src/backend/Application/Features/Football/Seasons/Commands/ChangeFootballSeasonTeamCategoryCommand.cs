using Application.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

/// <summary>
/// Moves a football season to another audience group (adult, youth, women). Allowed in any season status.
/// </summary>
/// <param name="SeasonId">Season to update.</param>
/// <param name="TeamCategory">New audience group.</param>
public record ChangeFootballSeasonTeamCategoryCommand(Guid SeasonId, TeamCategory TeamCategory) : IRequest<Result>;
