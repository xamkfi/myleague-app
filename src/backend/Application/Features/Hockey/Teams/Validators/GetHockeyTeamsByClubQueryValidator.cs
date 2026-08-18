using Application.Features.Hockey.Teams.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="GetHockeyTeamsByClubQuery"/>.
/// </summary>
public class GetHockeyTeamsByClubQueryValidator : AbstractValidator<GetHockeyTeamsByClubQuery>
{
    public GetHockeyTeamsByClubQueryValidator()
    {
        RuleFor(x => x.ClubId).NotEmpty().WithMessage("Club id is required.");
    }
}
