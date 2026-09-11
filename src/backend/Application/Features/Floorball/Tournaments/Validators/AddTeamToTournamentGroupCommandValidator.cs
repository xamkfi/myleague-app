using Application.Features.Floorball.Tournaments.Commands;
using FluentValidation;
using System;

namespace Application.Features.Floorball.Tournaments.Validators;

/// <summary>
/// Validator for AddTeamToTournamentGroupCommand
/// </summary>
public class AddTeamToTournamentGroupCommandValidator : AbstractValidator<AddTeamToTournamentGroupCommand>
{
    public AddTeamToTournamentGroupCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty().WithMessage("Tournament ID is required")
            .NotEqual(Guid.Empty).WithMessage("Tournament ID cannot be empty");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("Group ID is required")
            .NotEqual(Guid.Empty).WithMessage("Group ID cannot be empty");

        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
    }
}
