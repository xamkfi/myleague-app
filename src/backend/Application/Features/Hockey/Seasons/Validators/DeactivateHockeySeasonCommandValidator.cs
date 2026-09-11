using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="DeactivateHockeySeasonCommand"/>.
/// </summary>
public class DeactivateHockeySeasonCommandValidator : AbstractValidator<DeactivateHockeySeasonCommand>
{
    public DeactivateHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
    }
}
