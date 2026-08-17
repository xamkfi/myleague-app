using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using MediatR;

namespace Application.Features.Hockey.Competitions.Commands;

/// <summary>
/// Command to add a Common Division link to a hockey competition.
/// </summary>
public record CreateHockeyCompetitionDivisionCommand(
    Guid CompetitionId,
    Guid DivisionId,
    string Name,
    int SortOrder) : IRequest<Result<HockeyCompetitionDto>>;
