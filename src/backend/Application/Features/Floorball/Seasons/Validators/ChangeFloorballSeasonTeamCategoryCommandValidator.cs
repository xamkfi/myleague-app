using Application.Features.Floorball.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

/// <summary>
/// Validator for <see cref="ChangeFloorballSeasonTeamCategoryCommand"/>.
/// </summary>
public class ChangeFloorballSeasonTeamCategoryCommandValidator : AbstractValidator<ChangeFloorballSeasonTeamCategoryCommand>
{
    public ChangeFloorballSeasonTeamCategoryCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season ID is required");
        RuleFor(x => x.TeamCategory).IsInEnum().WithMessage("Team category is not valid");
    }
}
