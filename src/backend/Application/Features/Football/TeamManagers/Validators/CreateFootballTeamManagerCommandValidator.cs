using Application.Features.Football.TeamManagers.Commands;
using FluentValidation;

namespace Application.Features.Football.TeamManagers.Validators;

/// <summary>
/// Validator for CreateFootballTeamManagerCommand
/// </summary>
public class CreateFootballTeamManagerCommandValidator : AbstractValidator<CreateFootballTeamManagerCommand>
{
    public CreateFootballTeamManagerCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required")
            .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");

        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
    }
} 
