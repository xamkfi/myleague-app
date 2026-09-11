using Application.Features.Football.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class CreateFootballSeasonCommandValidator : AbstractValidator<CreateFootballSeasonCommand>
{
    public CreateFootballSeasonCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Season name is required")
            .MaximumLength(100).WithMessage("Season name cannot exceed 100 characters");

        RuleFor(x => x.DivisionIds)
            .NotNull().WithMessage("At least one division is required")
            .Must(divisionIds => divisionIds != null && divisionIds.Any()).WithMessage("At least one division must be specified")
            .Must(divisionIds => divisionIds != null && divisionIds.All(id => id != Guid.Empty)).WithMessage("All division IDs must be valid");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .Must(BeValidDate).WithMessage("Start date must be a valid date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .Must(BeValidDate).WithMessage("End date must be a valid date")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.NumberOfHalves)
            .InclusiveBetween(1, 4).WithMessage("Number of halves must be between 1 and 4");

        RuleFor(x => x.HalfDurationMinutes)
            .InclusiveBetween(1, 60).WithMessage("Half duration must be between 1 and 60 minutes");

        RuleFor(x => x.PlayersOnField)
            .InclusiveBetween(5, 11).WithMessage("Players on field must be between 5 and 11");

        RuleFor(x => x.MaxSubstitutions)
            .InclusiveBetween(0, 99).WithMessage("Max substitutions must be between 0 (unlimited) and 99");

        RuleFor(x => x.ExtraTimeHalfCount)
            .InclusiveBetween(1, 2).WithMessage("Extra-time half count must be 1 or 2")
            .When(x => x.AllowExtraTime);

        RuleFor(x => x.ExtraTimeHalfDurationMinutes)
            .InclusiveBetween(1, 30).WithMessage("Extra-time half duration must be between 1 and 30 minutes")
            .When(x => x.AllowExtraTime);

        RuleFor(x => x.WinPoints).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DrawPoints).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LossPoints).GreaterThanOrEqualTo(0);
    }

    private static bool BeValidDate(DateTime date) => date != default;
}
