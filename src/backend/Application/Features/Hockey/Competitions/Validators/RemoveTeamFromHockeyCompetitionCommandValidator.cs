using Application.Features.Hockey.Competitions.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

/// <summary>
/// Validator for <see cref="RemoveTeamFromHockeyCompetitionCommand"/>.
/// </summary>
public class RemoveTeamFromHockeyCompetitionCommandValidator
    : AbstractValidator<RemoveTeamFromHockeyCompetitionCommand>
{
    public RemoveTeamFromHockeyCompetitionCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.TeamId).NotEmpty();
    }
}
