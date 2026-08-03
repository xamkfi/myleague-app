using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="OpenHockeyTournamentRegistrationCommand"/>.
/// </summary>
public class OpenHockeyTournamentRegistrationCommandValidator : AbstractValidator<OpenHockeyTournamentRegistrationCommand>
{
    public OpenHockeyTournamentRegistrationCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
