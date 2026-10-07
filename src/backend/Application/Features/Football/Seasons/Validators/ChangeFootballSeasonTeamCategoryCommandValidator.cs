using Application.Features.Football.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

/// <summary>
/// Validator for <see cref="ChangeFootballSeasonTeamCategoryCommand"/>.
/// </summary>
public class ChangeFootballSeasonTeamCategoryCommandValidator : AbstractValidator<ChangeFootballSeasonTeamCategoryCommand>
{
    public ChangeFootballSeasonTeamCategoryCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season ID is required");
        RuleFor(x => x.TeamCategory).IsInEnum().WithMessage("Team category is not valid");
    }
}
