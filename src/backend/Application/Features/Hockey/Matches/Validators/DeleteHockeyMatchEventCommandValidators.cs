using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class DeleteHockeyGoalCommandValidator : AbstractValidator<DeleteHockeyGoalCommand>
{
    public DeleteHockeyGoalCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.GoalEventId).NotEmpty();
    }
}

public class DeleteHockeyPenaltyCommandValidator : AbstractValidator<DeleteHockeyPenaltyCommand>
{
    public DeleteHockeyPenaltyCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.PenaltyEventId).NotEmpty();
    }
}

public class DeleteHockeyShotCommandValidator : AbstractValidator<DeleteHockeyShotCommand>
{
    public DeleteHockeyShotCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.ShotEventId).NotEmpty();
    }
}
