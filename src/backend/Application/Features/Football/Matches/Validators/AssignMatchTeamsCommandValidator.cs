using Application.Features.Football.Matches.Commands;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

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
