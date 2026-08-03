using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="PublishHockeyTournamentCommand"/>.
/// </summary>
public class PublishHockeyTournamentCommandValidator : AbstractValidator<PublishHockeyTournamentCommand>
{
    public PublishHockeyTournamentCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
