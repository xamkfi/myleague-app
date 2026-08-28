using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="CreateHockeyPlayoffSeriesCommand"/>.
/// </summary>
public class CreateHockeyPlayoffSeriesCommandValidator : AbstractValidator<CreateHockeyPlayoffSeriesCommand>
{
    public CreateHockeyPlayoffSeriesCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
        RuleFor(x => x.Round).IsInEnum();
        RuleFor(x => x.SeriesOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BestOf).GreaterThanOrEqualTo(1);
    }
}
