using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="CreateHockeySeasonCommand"/>.
/// </summary>
public class CreateHockeySeasonCommandValidator : AbstractValidator<CreateHockeySeasonCommand>
{
    public CreateHockeySeasonCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Season name is required.")
            .MaximumLength(200).WithMessage("Season name cannot exceed 200 characters.");

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateTime)).WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEqual(default(DateTime)).WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.SeasonCode)
            .MaximumLength(50).WithMessage("Season code cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SeasonCode));
    }
}
