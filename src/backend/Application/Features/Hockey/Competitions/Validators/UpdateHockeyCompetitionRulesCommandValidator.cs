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
        RuleFor(x => x.Rules).NotNull();
        RuleFor(x => x.Rules.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Rules.RuleBookSource).IsInEnum();
        RuleFor(x => x.Rules.RuleBookVersion).MaximumLength(50).When(x => x.Rules.RuleBookVersion is not null);
    }
}
