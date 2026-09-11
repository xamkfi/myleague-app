using Application.Features.Hockey.Statistics.Commands;
using Application.Features.Hockey.Statistics.Queries;
using Domain.Enums.Hockey.Statistics;
using FluentValidation;

namespace Application.Features.Hockey.Statistics.Validators;

public class RecalculateHockeyMatchStatisticsCommandValidator
    : AbstractValidator<RecalculateHockeyMatchStatisticsCommand>
{
    public RecalculateHockeyMatchStatisticsCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
    }
}

public class RecalculateHockeyCompetitionStatisticsCommandValidator
    : AbstractValidator<RecalculateHockeyCompetitionStatisticsCommand>
{
    public RecalculateHockeyCompetitionStatisticsCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.CompetitionDivisionId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.Division);
        RuleFor(x => x.TournamentGroupId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.TournamentGroup);
        RuleFor(x => x.PlayoffSeriesId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.PlayoffSeries);
        RuleFor(x => x)
            .Must(x => x.Scope != HockeyStatisticsScope.Competition
                || (x.CompetitionDivisionId is null && x.TournamentGroupId is null && x.PlayoffSeriesId is null))
            .WithMessage("Competition scope cannot include division, group, or playoff series ids.");
    }
}

public class ResetHockeyCompetitionStatisticsCommandValidator
    : AbstractValidator<ResetHockeyCompetitionStatisticsCommand>
{
    public ResetHockeyCompetitionStatisticsCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.Scope!).IsInEnum().When(x => x.Scope is not null);
        RuleFor(x => x.CompetitionDivisionId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.Division);
        RuleFor(x => x.TournamentGroupId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.TournamentGroup);
        RuleFor(x => x.PlayoffSeriesId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.PlayoffSeries);
    }
}

public class GetHockeyMatchStatisticsQueryValidator : AbstractValidator<GetHockeyMatchStatisticsQuery>
{
    public GetHockeyMatchStatisticsQueryValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
    }
}

public class GetHockeyCompetitionStandingsQueryValidator : AbstractValidator<GetHockeyCompetitionStandingsQuery>
{
    public GetHockeyCompetitionStandingsQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
    }
}

public class GetHockeyDivisionStandingsQueryValidator : AbstractValidator<GetHockeyDivisionStandingsQuery>
{
    public GetHockeyDivisionStandingsQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.CompetitionDivisionId).NotEmpty();
    }
}

public class GetHockeyTournamentGroupStandingsQueryValidator
    : AbstractValidator<GetHockeyTournamentGroupStandingsQuery>
{
    public GetHockeyTournamentGroupStandingsQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.TournamentGroupId).NotEmpty();
    }
}

public class GetHockeyPlayoffSeriesStatisticsQueryValidator
    : AbstractValidator<GetHockeyPlayoffSeriesStatisticsQuery>
{
    public GetHockeyPlayoffSeriesStatisticsQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.PlayoffSeriesId).NotEmpty();
    }
}

public class GetHockeyTeamCompetitionStatisticsQueryValidator
    : AbstractValidator<GetHockeyTeamCompetitionStatisticsQuery>
{
    public GetHockeyTeamCompetitionStatisticsQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.CompetitionDivisionId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.Division);
        RuleFor(x => x.TournamentGroupId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.TournamentGroup);
        RuleFor(x => x.PlayoffSeriesId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.PlayoffSeries);
    }
}

public class GetHockeyPlayerCompetitionStatisticsQueryValidator
    : AbstractValidator<GetHockeyPlayerCompetitionStatisticsQuery>
{
    public GetHockeyPlayerCompetitionStatisticsQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .When(x => x.TeamId is not null);
        RuleFor(x => x.TeamId)
            .NotEmpty()
            .When(x => x.PlayerId is not null);
        RuleFor(x => x.CompetitionDivisionId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.Division);
        RuleFor(x => x.TournamentGroupId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.TournamentGroup);
        RuleFor(x => x.PlayoffSeriesId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.PlayoffSeries);
    }
}

public class GetHockeyGoalieCompetitionStatisticsQueryValidator
    : AbstractValidator<GetHockeyGoalieCompetitionStatisticsQuery>
{
    public GetHockeyGoalieCompetitionStatisticsQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .When(x => x.TeamId is not null);
        RuleFor(x => x.TeamId)
            .NotEmpty()
            .When(x => x.PlayerId is not null);
        RuleFor(x => x.CompetitionDivisionId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.Division);
        RuleFor(x => x.TournamentGroupId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.TournamentGroup);
        RuleFor(x => x.PlayoffSeriesId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.PlayoffSeries);
    }
}

public class GetHockeyTopScorersQueryValidator : AbstractValidator<GetHockeyTopScorersQuery>
{
    public GetHockeyTopScorersQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.TopN).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.CompetitionDivisionId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.Division);
        RuleFor(x => x.TournamentGroupId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.TournamentGroup);
        RuleFor(x => x.PlayoffSeriesId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.PlayoffSeries);
    }
}

public class GetHockeyTopGoaliesQueryValidator : AbstractValidator<GetHockeyTopGoaliesQuery>
{
    public GetHockeyTopGoaliesQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.TopN).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.MinimumGamesPlayed).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CompetitionDivisionId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.Division);
        RuleFor(x => x.TournamentGroupId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.TournamentGroup);
        RuleFor(x => x.PlayoffSeriesId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.PlayoffSeries);
    }
}

public class GetHockeyCompetitionStatisticsSummaryQueryValidator
    : AbstractValidator<GetHockeyCompetitionStatisticsSummaryQuery>
{
    public GetHockeyCompetitionStatisticsSummaryQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.TopN).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.CompetitionDivisionId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.Division);
        RuleFor(x => x.TournamentGroupId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.TournamentGroup);
        RuleFor(x => x.PlayoffSeriesId)
            .NotEmpty()
            .When(x => x.Scope == HockeyStatisticsScope.PlayoffSeries);
    }
}
