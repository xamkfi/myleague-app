using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validator for CreateFloorballMatchCommand
/// </summary>
public class CreateFloorballMatchCommandValidator : AbstractValidator<CreateFloorballMatchCommand>
{
    public CreateFloorballMatchCommandValidator()
    {
        RuleFor(x => x.SeasonId)
            .NotEmpty().WithMessage("Season ID is required")
            .NotEqual(Guid.Empty).WithMessage("Season ID cannot be empty");

        RuleFor(x => x.HomeTeamId)
            .NotEmpty().WithMessage("Home team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Home team ID cannot be empty");

        RuleFor(x => x.AwayTeamId)
            .NotEmpty().WithMessage("Away team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Away team ID cannot be empty")
            .NotEqual(x => x.HomeTeamId).WithMessage("Away team must be different from home team");

        //TODO: Temporary disable for old data import
        //RuleFor(x => x.ScheduledDateTime)
        //    .NotEmpty().WithMessage("Scheduled date and time is required")
        //    .GreaterThan(DateTime.UtcNow).WithMessage("Scheduled date and time must be in the future");

        RuleFor(x => x.Venue)
            .MaximumLength(100).WithMessage("Venue name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Venue));
    }
} 
