using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="UpdateHockeySeasonCommand"/>.
/// </summary>
public class UpdateHockeySeasonCommandValidator : AbstractValidator<UpdateHockeySeasonCommand>
{
    public UpdateHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(200);
        RuleFor(x => x.StartDate).NotEqual(default(DateTime));
        RuleFor(x => x.EndDate).NotEqual(default(DateTime)).GreaterThan(x => x.StartDate);
        RuleFor(x => x.SeasonCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.SeasonCode));
    }
}
