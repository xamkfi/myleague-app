using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Floorball;
using Domain.Enums.Football;
using Domain.Enums.Hockey.Competitions;

namespace Application.Common;

/// <summary>
/// Public catalogue rules. Draft hockey competitions and draft tournaments stay in admin.
/// Floorball and football seasons have no draft status: a season that is neither active nor completed is the public equivalent.
/// </summary>
public static class PublicCompetitionVisibility
{
    public static bool IsPublicSeason(FloorballSeason season) => season.IsActive || season.IsCompleted;

    public static bool IsPublicSeason(FootballSeason season) => season.IsActive || season.IsCompleted;

    public static bool IsPublicTournament(FloorballTournament tournament) =>
        tournament.TournamentStatus != FloorballTournamentStatus.Draft;

    public static bool IsPublicTournament(FootballTournament tournament) =>
        tournament.TournamentStatus != FootballTournamentStatus.Draft;

    public static bool IsPublic(HockeyCompetition competition) =>
        competition.Status != HockeyCompetitionStatus.Draft;

    public static bool IsPublicMatch(FloorballCompetition? competition) =>
        competition switch
        {
            FloorballTournament tournament => IsPublicTournament(tournament),
            FloorballSeason season => IsPublicSeason(season),
            _ => false
        };

    public static bool IsPublicMatch(FootballCompetition? competition) =>
        competition switch
        {
            FootballTournament tournament => IsPublicTournament(tournament),
            FootballSeason season => IsPublicSeason(season),
            _ => false
        };
}
