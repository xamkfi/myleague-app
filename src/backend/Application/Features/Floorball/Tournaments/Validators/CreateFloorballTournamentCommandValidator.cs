using Application.Features.Floorball.Tournaments.Commands;
using FluentValidation;
using System;

namespace Application.Features.Floorball.Tournaments.Validators;

/// <summary>
/// Validator for CreateFloorballTournamentCommand
/// </summary>
public class CreateFloorballTournamentCommandValidator : AbstractValidator<CreateFloorballTournamentCommand>
{
    public CreateFloorballTournamentCommandValidator()
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
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.GroupStageNumberOfPeriods)
            .InclusiveBetween(1, 5).WithMessage("Group stage number of periods must be between 1 and 5");

        RuleFor(x => x.GroupStagePeriodDurationMinutes)
            .InclusiveBetween(1, 60).WithMessage("Group stage period duration must be between 1 and 60 minutes");

        RuleFor(x => x.GroupStageOvertimeDurationMinutes)
            .InclusiveBetween(1, 30).WithMessage("Group stage overtime duration must be between 1 and 30 minutes")
            .When(x => x.GroupStageAllowOvertime);

        RuleFor(x => x.PlayoffNumberOfPeriods)
            .InclusiveBetween(1, 5).WithMessage("Playoff number of periods must be between 1 and 5");

        RuleFor(x => x.PlayoffPeriodDurationMinutes)
            .InclusiveBetween(1, 60).WithMessage("Playoff period duration must be between 1 and 60 minutes");

        RuleFor(x => x.PlayoffOvertimeDurationMinutes)
            .InclusiveBetween(1, 30).WithMessage("Playoff overtime duration must be between 1 and 30 minutes")
            .When(x => x.PlayoffAllowOvertime);

        RuleFor(x => x.TeamsAdvancingPerGroup)
            .InclusiveBetween(1, 8).WithMessage("Teams advancing per group must be between 1 and 8");
    }

    private bool BeValidDate(DateTime date)
    {
        return date != default;
    }
}
