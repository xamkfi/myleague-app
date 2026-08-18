using Domain.Enums.Hockey.Statistics;

namespace Domain.Entities.Hockey.Statistics;

internal static class HockeyStatisticsScopeValidator
{
    public static void Validate(
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId)
    {
        if (competitionDivisionId == Guid.Empty)
            throw new ArgumentException("Competition division id cannot be empty.", nameof(competitionDivisionId));
        if (tournamentGroupId == Guid.Empty)
            throw new ArgumentException("Tournament group id cannot be empty.", nameof(tournamentGroupId));
        if (playoffSeriesId == Guid.Empty)
            throw new ArgumentException("Playoff series id cannot be empty.", nameof(playoffSeriesId));

        switch (scope)
        {
            case HockeyStatisticsScope.Competition:
                if (competitionDivisionId is not null || tournamentGroupId is not null || playoffSeriesId is not null)
                    throw new InvalidOperationException("Competition scope cannot reference division, group or playoff series.");
                break;
            case HockeyStatisticsScope.Division:
                if (competitionDivisionId is null)
                    throw new InvalidOperationException("Division scope requires a competition division id.");
                if (tournamentGroupId is not null || playoffSeriesId is not null)
                    throw new InvalidOperationException("Division scope cannot reference tournament group or playoff series.");
                break;
            case HockeyStatisticsScope.TournamentGroup:
                if (tournamentGroupId is null)
                    throw new InvalidOperationException("Tournament group scope requires a tournament group id.");
                if (competitionDivisionId is not null || playoffSeriesId is not null)
                    throw new InvalidOperationException("Tournament group scope cannot reference division or playoff series.");
                break;
            case HockeyStatisticsScope.PlayoffSeries:
                if (playoffSeriesId is null)
                    throw new InvalidOperationException("Playoff series scope requires a playoff series id.");
                if (competitionDivisionId is not null || tournamentGroupId is not null)
                    throw new InvalidOperationException("Playoff series scope cannot reference division or tournament group.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unknown statistics scope.");
        }
    }
}

internal static class HockeyStatisticsMath
{
    public static decimal Percentage(int numerator, int denominator)
    {
        if (denominator <= 0)
            return 0m;
        return Math.Round((decimal)numerator / denominator * 100m, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal GoalsAgainstAverage(int goalsAgainst, int minutesPlayed)
    {
        if (minutesPlayed <= 0)
            return 0m;
        return Math.Round((decimal)goalsAgainst * 60m / minutesPlayed, 2, MidpointRounding.AwayFromZero);
    }

    public static void EnsureNonNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
    }
}
