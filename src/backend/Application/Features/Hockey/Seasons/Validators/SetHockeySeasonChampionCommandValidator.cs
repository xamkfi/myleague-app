using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="SetHockeySeasonChampionCommand"/>.
/// </summary>
public class SetHockeySeasonChampionCommandValidator : AbstractValidator<SetHockeySeasonChampionCommand>
{
    public SetHockeySeasonChampionCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.ChampionCompetitionTeamId).NotEmpty().WithMessage("Champion competition team id is required.");
    }
}
