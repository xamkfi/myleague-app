using Application.Features.Football.Matches.Commands;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

public class CompleteFootballMatchCommandValidator : AbstractValidator<CompleteFootballMatchCommand>
{
    public CompleteFootballMatchCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");
    }
}
