using Application.Features.Floorball.Teams.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Teams.Validators;

/// <summary>
/// Validator for UpdateTeamPlayerStatsCommand
/// </summary>
public class UpdateTeamPlayerStatsCommandValidator : AbstractValidator<UpdateTeamPlayerStatsCommand>
{
    public UpdateTeamPlayerStatsCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");

        RuleFor(x => x.PlayerId)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");

        RuleFor(x => x.GamesPlayed)
            .GreaterThanOrEqualTo(0).WithMessage("Games played cannot be negative");

        RuleFor(x => x.Goals)
            .GreaterThanOrEqualTo(0).WithMessage("Goals cannot be negative");

        RuleFor(x => x.Assists)
            .GreaterThanOrEqualTo(0).WithMessage("Assists cannot be negative");

        RuleFor(x => x.PenaltyMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Penalty minutes cannot be negative");
    }
} 
