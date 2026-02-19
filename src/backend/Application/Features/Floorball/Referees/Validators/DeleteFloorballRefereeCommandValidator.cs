using Application.Commands.Floorball.Referee;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Referee;

/// <summary>
/// Validator for DeleteFloorballRefereeCommand
/// </summary>
public class DeleteFloorballRefereeCommandValidator : AbstractValidator<DeleteFloorballRefereeCommand>
{
    public DeleteFloorballRefereeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Referee ID is required")
            .NotEqual(Guid.Empty).WithMessage("Referee ID cannot be empty");
    }
} 