using Application.Features.Hockey.Competitions.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

/// <summary>
/// Validator for <see cref="AddTeamToHockeyCompetitionDivisionCommand"/>.
/// </summary>
public class AddTeamToHockeyCompetitionDivisionCommandValidator
    : AbstractValidator<AddTeamToHockeyCompetitionDivisionCommand>
{
    public AddTeamToHockeyCompetitionDivisionCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.CompetitionDivisionId).NotEmpty();
        RuleFor(x => x.CompetitionTeamId).NotEmpty();
    }
}
