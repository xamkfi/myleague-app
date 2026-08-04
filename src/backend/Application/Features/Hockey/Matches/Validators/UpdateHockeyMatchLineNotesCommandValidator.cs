using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class UpdateHockeyMatchLineNotesCommandValidator : AbstractValidator<UpdateHockeyMatchLineNotesCommand>
{
    public UpdateHockeyMatchLineNotesCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchLineId).NotEmpty();
    }
}
