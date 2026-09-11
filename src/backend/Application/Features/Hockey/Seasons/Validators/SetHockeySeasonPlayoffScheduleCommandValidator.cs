using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="SetHockeySeasonPlayoffScheduleCommand"/>.
/// </summary>
public class SetHockeySeasonPlayoffScheduleCommandValidator
    : AbstractValidator<SetHockeySeasonPlayoffScheduleCommand>
{
    public SetHockeySeasonPlayoffScheduleCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.Slots).NotNull();
    }
}
