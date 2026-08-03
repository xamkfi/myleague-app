using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="AddTeamToHockeySeasonDivisionCommand"/>.
/// </summary>
public class AddTeamToHockeySeasonDivisionCommandValidator : AbstractValidator<AddTeamToHockeySeasonDivisionCommand>
{
    public AddTeamToHockeySeasonDivisionCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.CompetitionDivisionId).NotEmpty().WithMessage("Competition division id is required.");
        RuleFor(x => x.CompetitionTeamId).NotEmpty().WithMessage("Competition team id is required.");
    }
}
