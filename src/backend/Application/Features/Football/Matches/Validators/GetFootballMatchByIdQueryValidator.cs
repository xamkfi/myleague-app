using Application.Features.Football.Matches.Queries;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

public class GetFootballMatchByIdQueryValidator : AbstractValidator<GetFootballMatchByIdQuery>
{
    public GetFootballMatchByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");
    }
}
