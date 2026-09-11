using Application.Features.Football.Teams.Commands;
using FluentValidation;

namespace Application.Features.Football.Teams.Validators;

/// <summary>
/// Validator for DeleteFootballTeamCommand
/// </summary>
public class DeleteFootballTeamCommandValidator : AbstractValidator<DeleteFootballTeamCommand>
{
    public DeleteFootballTeamCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
    }
} 
