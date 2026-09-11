using Application.Features.Floorball.Referees.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Referees.Validators;

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
