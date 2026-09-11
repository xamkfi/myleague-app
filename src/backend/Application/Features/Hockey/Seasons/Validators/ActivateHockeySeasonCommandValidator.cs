using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="ActivateHockeySeasonCommand"/>.
/// </summary>
public class ActivateHockeySeasonCommandValidator : AbstractValidator<ActivateHockeySeasonCommand>
{
    public ActivateHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
    }
}
