using Application.Features.Hockey.Competitions.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

/// <summary>
/// Validator for <see cref="GetHockeyCompetitionByIdQuery"/>.
/// </summary>
public class GetHockeyCompetitionByIdQueryValidator : AbstractValidator<GetHockeyCompetitionByIdQuery>
{
    public GetHockeyCompetitionByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Competition id is required.");
    }
}
