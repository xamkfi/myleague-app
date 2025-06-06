using Application.Commands.Floorball.Match;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Match;

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