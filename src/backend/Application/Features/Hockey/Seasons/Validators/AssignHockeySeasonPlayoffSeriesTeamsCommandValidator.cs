using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="AssignHockeySeasonPlayoffSeriesTeamsCommand"/>.
/// </summary>
public class AssignHockeySeasonPlayoffSeriesTeamsCommandValidator
    : AbstractValidator<AssignHockeySeasonPlayoffSeriesTeamsCommand>
{
    public AssignHockeySeasonPlayoffSeriesTeamsCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.SeriesId).NotEmpty().WithMessage("Series id is required.");
        RuleFor(x => x.HomeCompetitionTeamId).NotEmpty().WithMessage("Home competition team id is required.");
        RuleFor(x => x.AwayCompetitionTeamId).NotEmpty().WithMessage("Away competition team id is required.");
    }
}
