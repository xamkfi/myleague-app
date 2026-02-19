using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validator for StartFloorballMatchCommand
/// </summary>
public class StartFloorballMatchCommandValidator : AbstractValidator<StartFloorballMatchCommand>
{
    public StartFloorballMatchCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");
    }
} 
