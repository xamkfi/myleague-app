using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="RemoveDivisionFromHockeySeasonCommand"/>.
/// </summary>
public class RemoveDivisionFromHockeySeasonCommandValidator : AbstractValidator<RemoveDivisionFromHockeySeasonCommand>
{
    public RemoveDivisionFromHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.CompetitionDivisionId).NotEmpty().WithMessage("Competition division id is required.");
    }
}
