using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="CompleteHockeySeasonCommand"/>.
/// </summary>
public class CompleteHockeySeasonCommandValidator : AbstractValidator<CompleteHockeySeasonCommand>
{
    public CompleteHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
    }
}
