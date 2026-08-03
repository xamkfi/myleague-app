using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="RemoveTeamFromHockeyTournamentCommand"/>.
/// </summary>
public class RemoveTeamFromHockeyTournamentCommandValidator : AbstractValidator<RemoveTeamFromHockeyTournamentCommand>
{
    public RemoveTeamFromHockeyTournamentCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team id is required.");
    }
}
