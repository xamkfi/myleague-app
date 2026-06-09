using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validates <see cref="AddOfficialToMatchCommand"/>. Both IDs must be non-empty Guids; match
/// and referee existence are checked by the handler since they require repository access.
/// </summary>
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
