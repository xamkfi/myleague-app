using Application.Features.Football.Matches.Commands;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

public class UpdateFootballMatchCommandValidator : AbstractValidator<UpdateFootballMatchCommand>
{
    public UpdateFootballMatchCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");

        RuleFor(x => x.ScheduledDateTime)
            .NotEmpty().WithMessage("Scheduled date and time is required")
            .Must(date => date != default).WithMessage("Scheduled date and time must be a valid date");

        RuleFor(x => x.Venue)
            .MaximumLength(100).WithMessage("Venue name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Venue));
    }
}
