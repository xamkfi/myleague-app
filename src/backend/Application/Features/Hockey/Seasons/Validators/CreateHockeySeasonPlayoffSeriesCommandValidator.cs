using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="CreateHockeySeasonPlayoffSeriesCommand"/>.
/// </summary>
public class CreateHockeySeasonPlayoffSeriesCommandValidator
    : AbstractValidator<CreateHockeySeasonPlayoffSeriesCommand>
{
    public CreateHockeySeasonPlayoffSeriesCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.Round).IsInEnum();
        RuleFor(x => x.SeriesOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BestOf).GreaterThanOrEqualTo(1);
    }
}
