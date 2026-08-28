using Application.Features.Football.Referees.Commands;
using FluentValidation;

namespace Application.Features.Football.Referees.Validators;

/// <summary>
/// Validator for DeleteFootballRefereeCommand
/// </summary>
public class DeleteFootballRefereeCommandValidator : AbstractValidator<DeleteFootballRefereeCommand>
{
    public DeleteFootballRefereeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Referee ID is required")
            .NotEqual(Guid.Empty).WithMessage("Referee ID cannot be empty");
    }
} 
