using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="ChangeHockeySeasonTeamCategoryCommand"/>.
/// </summary>
public class ChangeHockeySeasonTeamCategoryCommandValidator : AbstractValidator<ChangeHockeySeasonTeamCategoryCommand>
{
    public ChangeHockeySeasonTeamCategoryCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season ID is required");
        RuleFor(x => x.TeamCategory).IsInEnum().WithMessage("Team category is not valid");
    }
}
