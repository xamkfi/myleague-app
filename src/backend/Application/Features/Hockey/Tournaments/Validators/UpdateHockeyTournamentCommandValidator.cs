using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="UpdateHockeyTournamentCommand"/>.
/// </summary>
public class UpdateHockeyTournamentCommandValidator : AbstractValidator<UpdateHockeyTournamentCommand>
{
    public UpdateHockeyTournamentCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(200);
        RuleFor(x => x.StartDate).NotEqual(default(DateTime));
        RuleFor(x => x.EndDate).NotEqual(default(DateTime)).GreaterThan(x => x.StartDate);
        RuleFor(x => x.Venue).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Venue));
        RuleFor(x => x.TeamCategory).IsInEnum();
    }
}
