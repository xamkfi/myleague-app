namespace Domain.Enums.Football;

/// <summary>
/// Identifies which concrete competition type a football match belongs to.
/// Used as a filter when querying matches across the TPH FootballCompetition hierarchy.
/// </summary>
public enum FootballCompetitionType
{
    Season = 0,
    Tournament = 1
}
