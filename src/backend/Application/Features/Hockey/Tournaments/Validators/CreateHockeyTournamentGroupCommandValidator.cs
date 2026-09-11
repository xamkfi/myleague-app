using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="CreateHockeyTournamentGroupCommand"/>.
/// </summary>
public class CreateHockeyTournamentGroupCommandValidator : AbstractValidator<CreateHockeyTournamentGroupCommand>
{
    public CreateHockeyTournamentGroupCommandValidator()
    {
        RuleFor(x => x.TournamentId)
            .NotEmpty().WithMessage("Tournament id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required.")
            .MaximumLength(200).WithMessage("Group name cannot exceed 200 characters.");
    }
}
