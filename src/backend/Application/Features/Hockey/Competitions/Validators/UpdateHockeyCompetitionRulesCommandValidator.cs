using Application.Features.Hockey.Competitions.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

/// <summary>
/// Validator for <see cref="UpdateHockeyCompetitionRulesCommand"/>.
/// </summary>
public class UpdateHockeyCompetitionRulesCommandValidator
    : AbstractValidator<UpdateHockeyCompetitionRulesCommand>
{
    public UpdateHockeyCompetitionRulesCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RuleBookSource).IsInEnum();
        RuleFor(x => x.RuleBookVersion).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.RuleBookVersion));
    }
}
