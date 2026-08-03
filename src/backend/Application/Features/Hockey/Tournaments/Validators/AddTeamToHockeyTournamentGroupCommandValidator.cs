using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="AddTeamToHockeyTournamentGroupCommand"/>.
/// </summary>
public class AddTeamToHockeyTournamentGroupCommandValidator : AbstractValidator<AddTeamToHockeyTournamentGroupCommand>
{
    public AddTeamToHockeyTournamentGroupCommandValidator()
    {
        RuleFor(x => x.TournamentId)
            .NotEmpty().WithMessage("Tournament id is required.");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("Group id is required.");

        RuleFor(x => x.CompetitionTeamId)
            .NotEmpty().WithMessage("Competition team id is required.");
    }
}
