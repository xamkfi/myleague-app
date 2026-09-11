using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="AssignHockeyPlayoffSeriesTeamsCommand"/>.
/// </summary>
public class AssignHockeyPlayoffSeriesTeamsCommandValidator : AbstractValidator<AssignHockeyPlayoffSeriesTeamsCommand>
{
    public AssignHockeyPlayoffSeriesTeamsCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty();
        RuleFor(x => x.SeriesId).NotEmpty();
        RuleFor(x => x.HomeCompetitionTeamId).NotEmpty();
        RuleFor(x => x.AwayCompetitionTeamId).NotEmpty()
            .NotEqual(x => x.HomeCompetitionTeamId)
            .WithMessage("Home and away competition teams must be different.");
    }
}
