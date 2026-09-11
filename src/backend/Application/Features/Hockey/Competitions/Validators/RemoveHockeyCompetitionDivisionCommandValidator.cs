using Application.Features.Hockey.Competitions.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

/// <summary>
/// Validator for <see cref="RemoveHockeyCompetitionDivisionCommand"/>.
/// </summary>
public class RemoveHockeyCompetitionDivisionCommandValidator
    : AbstractValidator<RemoveHockeyCompetitionDivisionCommand>
{
    public RemoveHockeyCompetitionDivisionCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.CompetitionDivisionId).NotEmpty();
    }
}
