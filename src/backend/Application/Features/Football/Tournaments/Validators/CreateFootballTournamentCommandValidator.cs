using Application.Features.Football.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Football.Tournaments.Validators;

/// <summary>
/// Validator for CreateFootballTournamentCommand
/// </summary>
public class CreateFootballTournamentCommandValidator : AbstractValidator<CreateFootballTournamentCommand>
{
    public CreateFootballTournamentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tournament name is required")
            .MaximumLength(200).WithMessage("Tournament name cannot exceed 200 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .Must(BeValidDate).WithMessage("Start date must be a valid date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .Must(BeValidDate).WithMessage("End date must be a valid date")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date cannot be before start date");

        RuleFor(x => x.GroupStageNumberOfHalves)
            .InclusiveBetween(1, 4).WithMessage("Group stage number of halves must be between 1 and 4");

        RuleFor(x => x.GroupStageHalfDurationMinutes)
            .InclusiveBetween(1, 60).WithMessage("Group stage half duration must be between 1 and 60 minutes");

        RuleFor(x => x.GroupStagePlayersOnField)
            .InclusiveBetween(5, 11).WithMessage("Group stage players on field must be between 5 and 11");

        RuleFor(x => x.GroupStageMaxSubstitutions)
            .InclusiveBetween(0, 99).WithMessage("Group stage max substitutions must be between 0 (unlimited) and 99");

        RuleFor(x => x.GroupStageExtraTimeHalfCount)
            .InclusiveBetween(1, 2).WithMessage("Group stage extra-time half count must be 1 or 2")
            .When(x => x.GroupStageAllowExtraTime);

        RuleFor(x => x.GroupStageExtraTimeHalfDurationMinutes)
            .InclusiveBetween(1, 30).WithMessage("Group stage extra-time half duration must be between 1 and 30 minutes")
            .When(x => x.GroupStageAllowExtraTime);

        RuleFor(x => x.PlayoffNumberOfHalves)
            .InclusiveBetween(1, 4).WithMessage("Playoff number of halves must be between 1 and 4");

        RuleFor(x => x.PlayoffHalfDurationMinutes)
            .InclusiveBetween(1, 60).WithMessage("Playoff half duration must be between 1 and 60 minutes");

        RuleFor(x => x.PlayoffPlayersOnField)
            .InclusiveBetween(5, 11).WithMessage("Playoff players on field must be between 5 and 11");

        RuleFor(x => x.PlayoffMaxSubstitutions)
            .InclusiveBetween(0, 99).WithMessage("Playoff max substitutions must be between 0 (unlimited) and 99");

        RuleFor(x => x.PlayoffExtraTimeHalfCount)
            .InclusiveBetween(1, 2).WithMessage("Playoff extra-time half count must be 1 or 2")
            .When(x => x.PlayoffAllowExtraTime);

        RuleFor(x => x.PlayoffExtraTimeHalfDurationMinutes)
            .InclusiveBetween(1, 30).WithMessage("Playoff extra-time half duration must be between 1 and 30 minutes")
            .When(x => x.PlayoffAllowExtraTime);

        RuleFor(x => x.TeamsAdvancingPerGroup)
            .InclusiveBetween(1, 8).WithMessage("Teams advancing per group must be between 1 and 8")
            .When(x => x.HasPlayoffStage);
    }

    private bool BeValidDate(DateTime date)
    {
        return date != default;
    }
}
