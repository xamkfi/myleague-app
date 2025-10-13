using Application.Commands.Floorball.Match;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Match;

/// <summary>
/// Validator for CompleteFloorballMatchCommand
/// </summary>
public class CompleteFloorballMatchCommandValidator : AbstractValidator<CompleteFloorballMatchCommand>
{
    public CompleteFloorballMatchCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");
    }
} 