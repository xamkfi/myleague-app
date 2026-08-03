using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="AddDivisionToHockeySeasonCommand"/>.
/// </summary>
public class AddDivisionToHockeySeasonCommandValidator : AbstractValidator<AddDivisionToHockeySeasonCommand>
{
    public AddDivisionToHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.DivisionId).NotEmpty().WithMessage("Division id is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Division name is required.").MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
