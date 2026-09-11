using Application.Features.Hockey.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="GetHockeySeasonByIdQuery"/>.
/// </summary>
public class GetHockeySeasonByIdQueryValidator : AbstractValidator<GetHockeySeasonByIdQuery>
{
    public GetHockeySeasonByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Season id is required.");
    }
}
