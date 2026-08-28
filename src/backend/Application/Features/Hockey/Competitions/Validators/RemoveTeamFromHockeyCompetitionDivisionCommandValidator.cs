using Application.Features.Hockey.Competitions.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

/// <summary>
/// Validator for <see cref="RemoveTeamFromHockeyCompetitionDivisionCommand"/>.
/// </summary>
public class RemoveTeamFromHockeyCompetitionDivisionCommandValidator
    : AbstractValidator<RemoveTeamFromHockeyCompetitionDivisionCommand>
{
    public RemoveTeamFromHockeyCompetitionDivisionCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.CompetitionDivisionId).NotEmpty();
        RuleFor(x => x.CompetitionTeamId).NotEmpty();
    }
}
