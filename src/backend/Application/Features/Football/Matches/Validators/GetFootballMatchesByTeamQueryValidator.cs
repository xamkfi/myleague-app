using Application.Features.Football.Matches.Queries;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

public class GetFootballMatchesByTeamQueryValidator : AbstractValidator<GetFootballMatchesByTeamQuery>
{
    public GetFootballMatchesByTeamQueryValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be less than or equal to end date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
