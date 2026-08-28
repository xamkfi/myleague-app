using Application.Features.Football.Matches.Queries;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

public class GetFootballMatchesBySeasonQueryValidator : AbstractValidator<GetFootballMatchesBySeasonQuery>
{
    public GetFootballMatchesBySeasonQueryValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty().WithMessage("Competition ID is required")
            .NotEqual(Guid.Empty).WithMessage("Competition ID cannot be empty");
    }
}
