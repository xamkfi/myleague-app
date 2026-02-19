using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

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
