using Domain.Enums.Hockey.Competitions;

namespace Domain.ValueObjects.Hockey.Matches;

/// <summary>
/// Defines how teams are sourced into a playoff bracket slot.
/// </summary>
public sealed class HockeyPlayoffScheduleSlot : IEquatable<HockeyPlayoffScheduleSlot>
{
    public HockeyPlayoffRound Round { get; private set; }
    public int SeriesOrder { get; private set; }
    public int MatchOrder { get; private set; }
    public HockeyPlayoffSourceType HomeSourceType { get; private set; }
    public HockeyPlayoffSourceType AwaySourceType { get; private set; }
    public Guid? HomeSourceGroupId { get; private set; }
    public Guid? AwaySourceGroupId { get; private set; }
    public Guid? HomeSourceSeriesId { get; private set; }
    public Guid? AwaySourceSeriesId { get; private set; }
    public int? HomeSourceRank { get; private set; }
    public int? AwaySourceRank { get; private set; }
    public Guid? ManualHomeCompetitionTeamId { get; private set; }
    public Guid? ManualAwayCompetitionTeamId { get; private set; }

    private HockeyPlayoffScheduleSlot() { }

    public HockeyPlayoffScheduleSlot(
        HockeyPlayoffRound round,
        int seriesOrder,
        int matchOrder,
        HockeyPlayoffSourceType homeSourceType,
        HockeyPlayoffSourceType awaySourceType,
        Guid? homeSourceGroupId = null,
        Guid? awaySourceGroupId = null,
        Guid? homeSourceSeriesId = null,
        Guid? awaySourceSeriesId = null,
        int? homeSourceRank = null,
        int? awaySourceRank = null,
        Guid? manualHomeCompetitionTeamId = null,
        Guid? manualAwayCompetitionTeamId = null)
    {
        if (seriesOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(seriesOrder), "Series order cannot be negative.");
        if (matchOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(matchOrder), "Match order cannot be negative.");

        Round = round;
        SeriesOrder = seriesOrder;
        MatchOrder = matchOrder;
        HomeSourceType = homeSourceType;
        AwaySourceType = awaySourceType;
        HomeSourceGroupId = homeSourceGroupId;
        AwaySourceGroupId = awaySourceGroupId;
        HomeSourceSeriesId = homeSourceSeriesId;
        AwaySourceSeriesId = awaySourceSeriesId;
        HomeSourceRank = homeSourceRank;
        AwaySourceRank = awaySourceRank;
        ManualHomeCompetitionTeamId = manualHomeCompetitionTeamId;
        ManualAwayCompetitionTeamId = manualAwayCompetitionTeamId;
    }

    public override bool Equals(object? obj) => Equals(obj as HockeyPlayoffScheduleSlot);

    public bool Equals(HockeyPlayoffScheduleSlot? other)
    {
        if (other is null) return false;
        return Round == other.Round
            && SeriesOrder == other.SeriesOrder
            && MatchOrder == other.MatchOrder
            && HomeSourceType == other.HomeSourceType
            && AwaySourceType == other.AwaySourceType
            && HomeSourceGroupId == other.HomeSourceGroupId
            && AwaySourceGroupId == other.AwaySourceGroupId
            && HomeSourceSeriesId == other.HomeSourceSeriesId
            && AwaySourceSeriesId == other.AwaySourceSeriesId
            && HomeSourceRank == other.HomeSourceRank
            && AwaySourceRank == other.AwaySourceRank
            && ManualHomeCompetitionTeamId == other.ManualHomeCompetitionTeamId
            && ManualAwayCompetitionTeamId == other.ManualAwayCompetitionTeamId;
    }

    public override int GetHashCode() =>
        HashCode.Combine(Round, SeriesOrder, MatchOrder, HomeSourceType, AwaySourceType,
            HomeSourceGroupId, AwaySourceGroupId, HomeSourceSeriesId);

    public static bool operator ==(HockeyPlayoffScheduleSlot? left, HockeyPlayoffScheduleSlot? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyPlayoffScheduleSlot? left, HockeyPlayoffScheduleSlot? right) => !(left == right);
}
