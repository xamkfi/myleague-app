using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="StartHockeyTournamentGroupStageCommand"/>.
/// </summary>
public class StartHockeyTournamentGroupStageCommandValidator : AbstractValidator<StartHockeyTournamentGroupStageCommand>
{
    public StartHockeyTournamentGroupStageCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
