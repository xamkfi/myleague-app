using Application.Features.Hockey.Players.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Players.Validators;

/// <summary>
/// Validator for <see cref="GetHockeyPlayerByIdQuery"/>.
/// </summary>
public class GetHockeyPlayerByIdQueryValidator : AbstractValidator<GetHockeyPlayerByIdQuery>
{
    public GetHockeyPlayerByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Player id is required.");
    }
}
