using Application.Commands.Floorball.Match;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Match;

/// <summary>
/// Validator for UpdateFloorballMatchCommand
/// </summary>
public class UpdateFloorballMatchCommandValidator : AbstractValidator<UpdateFloorballMatchCommand>
{
    public UpdateFloorballMatchCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");

        RuleFor(x => x.ScheduledDateTime)
            .NotEmpty().WithMessage("Scheduled date and time is required")
            .Must(BeValidDate).WithMessage("Scheduled date and time must be a valid date")
            .GreaterThan(DateTime.UtcNow).WithMessage("Scheduled date and time must be in the future");

        RuleFor(x => x.Venue)
            .MaximumLength(100).WithMessage("Venue name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Venue));
    }

    private bool BeValidDate(DateTime date)
    {
        return date != default && date.Kind == DateTimeKind.Utc;
    }
} 