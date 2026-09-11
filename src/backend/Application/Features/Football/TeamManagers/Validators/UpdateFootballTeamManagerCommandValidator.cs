using Application.Features.Football.TeamManagers.Commands;
using FluentValidation;

namespace Application.Features.Football.TeamManagers.Validators;

/// <summary>
/// Validator for UpdateFootballTeamManagerCommand
/// </summary>
public class UpdateFootballTeamManagerCommandValidator : AbstractValidator<UpdateFootballTeamManagerCommand>
{
    public UpdateFootballTeamManagerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Team manager ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team manager ID cannot be empty");
    }
} 
