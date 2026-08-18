using Application.Features.Hockey.Competitions.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

/// <summary>
/// Validator for <see cref="CreateHockeyCompetitionDivisionCommand"/>.
/// </summary>
public class CreateHockeyCompetitionDivisionCommandValidator
    : AbstractValidator<CreateHockeyCompetitionDivisionCommand>
{
    public CreateHockeyCompetitionDivisionCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.DivisionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
