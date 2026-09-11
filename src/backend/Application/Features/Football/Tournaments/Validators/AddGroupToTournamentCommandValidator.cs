using Application.Features.Football.Tournaments.Commands;
using FluentValidation;
using System;

namespace Application.Features.Football.Tournaments.Validators;

/// <summary>
/// Validator for AddGroupToTournamentCommand
/// </summary>
public class AddGroupToTournamentCommandValidator : AbstractValidator<AddGroupToTournamentCommand>
{
    public AddGroupToTournamentCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty().WithMessage("Tournament ID is required")
            .NotEqual(Guid.Empty).WithMessage("Tournament ID cannot be empty");

        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group name is required")
            .MaximumLength(100).WithMessage("Group name cannot exceed 100 characters");
    }
}
