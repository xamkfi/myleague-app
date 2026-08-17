using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="StartHockeyTournamentPlayoffStageCommand"/>.
/// </summary>
public class StartHockeyTournamentPlayoffStageCommandValidator : AbstractValidator<StartHockeyTournamentPlayoffStageCommand>
{
    public StartHockeyTournamentPlayoffStageCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
