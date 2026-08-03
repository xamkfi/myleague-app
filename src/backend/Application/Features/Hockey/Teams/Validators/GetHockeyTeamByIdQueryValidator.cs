using Application.Features.Hockey.Teams.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

public class GetHockeyTeamByIdQueryValidator : AbstractValidator<GetHockeyTeamByIdQuery>
{
    public GetHockeyTeamByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Team id is required.");
    }
}
