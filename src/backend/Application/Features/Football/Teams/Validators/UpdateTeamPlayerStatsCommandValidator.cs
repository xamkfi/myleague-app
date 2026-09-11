using Application.Features.Football.Teams.Commands;
using FluentValidation;

namespace Application.Features.Football.Teams.Validators;

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

        RuleFor(x => x.YellowCards)
            .GreaterThanOrEqualTo(0).WithMessage("Yellow cards cannot be negative");

        RuleFor(x => x.RedCards)
            .GreaterThanOrEqualTo(0).WithMessage("Red cards cannot be negative");
    }
} 
