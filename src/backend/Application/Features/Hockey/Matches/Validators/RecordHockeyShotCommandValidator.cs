using Application.Features.Hockey.Matches.Commands;
using Domain.Enums.Hockey.Matches;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyShotCommandValidator : AbstractValidator<RecordHockeyShotCommand>
{
    public const int MaxBulkCount = 99;

    public RecordHockeyShotCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.ShootingMatchTeamId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShotResult).IsInEnum();
        RuleFor(x => x.Count).InclusiveBetween(1, MaxBulkCount);

        When(x => x.Count > 1, () =>
        {
            RuleFor(x => x.ShotResult)
                .Equal(HockeyShotResult.Saved)
                .WithMessage("Only saved shots can be recorded in bulk.");
            RuleFor(x => x.GoalieActivePlayerId)
                .NotNull()
                .WithMessage("A goalie is required when recording saves in bulk.");
        });
    }
}
