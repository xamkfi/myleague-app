using Application.Features.Hockey.Tournaments.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="GetHockeyTournamentByIdQuery"/>.
/// </summary>
public class GetHockeyTournamentByIdQueryValidator : AbstractValidator<GetHockeyTournamentByIdQuery>
{
    public GetHockeyTournamentByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Tournament id is required.");
    }
}
