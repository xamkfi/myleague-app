using Application.Features.Football.Matches.Commands;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

public class AddOfficialToMatchCommandValidator : AbstractValidator<AddOfficialToMatchCommand>
{
    public AddOfficialToMatchCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");

        RuleFor(x => x.RefereeId)
            .NotEmpty().WithMessage("Referee ID is required")
            .NotEqual(Guid.Empty).WithMessage("Referee ID cannot be empty");
    }
}
