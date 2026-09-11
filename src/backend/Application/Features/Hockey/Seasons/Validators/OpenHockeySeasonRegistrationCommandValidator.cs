using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="OpenHockeySeasonRegistrationCommand"/>.
/// </summary>
public class OpenHockeySeasonRegistrationCommandValidator : AbstractValidator<OpenHockeySeasonRegistrationCommand>
{
    public OpenHockeySeasonRegistrationCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
    }
}
