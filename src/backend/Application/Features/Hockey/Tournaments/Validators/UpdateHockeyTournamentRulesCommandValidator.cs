using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="UpdateHockeyTournamentRulesCommand"/>.
/// </summary>
public class UpdateHockeyTournamentRulesCommandValidator : AbstractValidator<UpdateHockeyTournamentRulesCommand>
{
    public UpdateHockeyTournamentRulesCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
        RuleFor(x => x.Format).IsInEnum();
        RuleFor(x => x.TeamsAdvancingPerGroup)
            .GreaterThanOrEqualTo(1)
            .When(x => x.HasPlayoffs)
            .WithMessage("Teams advancing per group must be at least 1 when playoffs are enabled.");
    }
}
