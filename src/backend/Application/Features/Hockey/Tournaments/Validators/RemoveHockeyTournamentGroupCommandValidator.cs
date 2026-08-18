using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="RemoveHockeyTournamentGroupCommand"/>.
/// </summary>
public class RemoveHockeyTournamentGroupCommandValidator : AbstractValidator<RemoveHockeyTournamentGroupCommand>
{
    public RemoveHockeyTournamentGroupCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
        RuleFor(x => x.GroupId).NotEmpty().WithMessage("Group id is required.");
    }
}
