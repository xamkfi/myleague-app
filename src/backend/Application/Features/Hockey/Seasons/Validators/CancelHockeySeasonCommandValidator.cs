using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="CancelHockeySeasonCommand"/>.
/// </summary>
public class CancelHockeySeasonCommandValidator : AbstractValidator<CancelHockeySeasonCommand>
{
    public CancelHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
    }
}
