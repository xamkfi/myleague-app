using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validates <see cref="AssignMatchTeamsCommand"/>. Both team slots are nullable (clearing is
/// allowed), but any provided ID must be non-empty and the two sides cannot reference the same
/// team. The match-existence and status checks live in the handler because they need repository
/// access.
/// </summary>
public class AssignMatchTeamsCommandValidator : AbstractValidator<AssignMatchTeamsCommand>
{
    public AssignMatchTeamsCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");

        RuleFor(x => x.HomeTeamId!.Value)
            .NotEqual(Guid.Empty).WithMessage("Home team ID cannot be empty")
            .When(x => x.HomeTeamId.HasValue);

        RuleFor(x => x.AwayTeamId!.Value)
            .NotEqual(Guid.Empty).WithMessage("Away team ID cannot be empty")
            .When(x => x.AwayTeamId.HasValue);

        RuleFor(x => x.AwayTeamId)
            .NotEqual(x => x.HomeTeamId).WithMessage("Home team and away team cannot be the same team")
            .When(x => x.HomeTeamId.HasValue && x.AwayTeamId.HasValue);
    }
}
