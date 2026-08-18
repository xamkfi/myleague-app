using Application.Features.Football.Matches.Commands;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

public class CreateFootballMatchCommandValidator : AbstractValidator<CreateFootballMatchCommand>
{
    public CreateFootballMatchCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty().WithMessage("Competition ID is required")
            .NotEqual(Guid.Empty).WithMessage("Competition ID cannot be empty");

        RuleFor(x => x.HomeTeamId!.Value)
            .NotEqual(Guid.Empty).WithMessage("Home team ID cannot be empty")
            .When(x => x.HomeTeamId.HasValue);

        RuleFor(x => x.AwayTeamId!.Value)
            .NotEqual(Guid.Empty).WithMessage("Away team ID cannot be empty")
            .When(x => x.AwayTeamId.HasValue);

        RuleFor(x => x.AwayTeamId)
            .NotEqual(x => x.HomeTeamId).WithMessage("Away team must be different from home team")
            .When(x => x.HomeTeamId.HasValue && x.AwayTeamId.HasValue);

        RuleFor(x => x.Venue)
            .MaximumLength(100).WithMessage("Venue name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Venue));
    }
}
