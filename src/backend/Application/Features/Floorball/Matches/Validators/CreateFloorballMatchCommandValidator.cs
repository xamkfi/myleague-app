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
        RuleFor(x => x.CompetitionId)
            .NotEmpty().WithMessage("Competition ID is required")
            .NotEqual(Guid.Empty).WithMessage("Competition ID cannot be empty");

        // Teams are optional at creation: fixtures can be published before the participants are
        // known (future league round, playoff slot before feeders resolve). When supplied each
        // team ID must still be non-empty, and the two sides cannot point at the same team.
        RuleFor(x => x.HomeTeamId!.Value)
            .NotEqual(Guid.Empty).WithMessage("Home team ID cannot be empty")
            .When(x => x.HomeTeamId.HasValue);

        RuleFor(x => x.AwayTeamId!.Value)
            .NotEqual(Guid.Empty).WithMessage("Away team ID cannot be empty")
            .When(x => x.AwayTeamId.HasValue);

        RuleFor(x => x.AwayTeamId)
            .NotEqual(x => x.HomeTeamId).WithMessage("Away team must be different from home team")
            .When(x => x.HomeTeamId.HasValue && x.AwayTeamId.HasValue);

        //TODO: Temporary disable for old data import
        //RuleFor(x => x.ScheduledDateTime)
        //    .NotEmpty().WithMessage("Scheduled date and time is required")
        //    .GreaterThan(DateTime.UtcNow).WithMessage("Scheduled date and time must be in the future");

        RuleFor(x => x.Venue)
            .MaximumLength(100).WithMessage("Venue name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Venue));
    }
} 
