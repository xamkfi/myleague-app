using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="RemoveTeamFromHockeySeasonDivisionCommand"/>.
/// </summary>
public class RemoveTeamFromHockeySeasonDivisionCommandValidator
    : AbstractValidator<RemoveTeamFromHockeySeasonDivisionCommand>
{
    public RemoveTeamFromHockeySeasonDivisionCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
        RuleFor(x => x.CompetitionDivisionId).NotEmpty().WithMessage("Competition division id is required.");
        RuleFor(x => x.CompetitionTeamId).NotEmpty().WithMessage("Competition team id is required.");
    }
}
