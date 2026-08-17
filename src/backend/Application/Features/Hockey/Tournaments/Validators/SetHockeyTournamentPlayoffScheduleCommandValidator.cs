using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="SetHockeyTournamentPlayoffScheduleCommand"/>.
/// </summary>
public class SetHockeyTournamentPlayoffScheduleCommandValidator : AbstractValidator<SetHockeyTournamentPlayoffScheduleCommand>
{
    public SetHockeyTournamentPlayoffScheduleCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty();
        RuleFor(x => x.Slots).NotNull();
    }
}
