using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class UpdateHockeyMatchScheduledStartCommandValidator : AbstractValidator<UpdateHockeyMatchScheduledStartCommand>
{
    public UpdateHockeyMatchScheduledStartCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.ScheduledStartTime).NotEmpty();
    }
}
