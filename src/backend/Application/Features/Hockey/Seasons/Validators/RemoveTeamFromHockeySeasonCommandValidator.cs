using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="RemoveTeamFromHockeySeasonCommand"/>.
/// </summary>
public class RemoveTeamFromHockeySeasonCommandValidator : AbstractValidator<RemoveTeamFromHockeySeasonCommand>
{
    public RemoveTeamFromHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team id is required.");
    }
}
