using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Floorball;
using Domain.Enums.Football;
using Domain.Enums.Hockey.Competitions;

namespace Application.Common;

/// <summary>
/// Public catalogue rules. Unpublished future seasons and draft tournaments stay in admin.
/// A season whose end date has passed is history and stays public, including SQL imports
/// that were never marked active or completed.
/// </summary>
public static class PublicCompetitionVisibility
{
    public static bool HasEnded(DateTime endDate) => endDate < DateTime.UtcNow;

    public static bool IsPublicSeason(FloorballSeason season) =>
        season.IsActive || season.IsCompleted || HasEnded(season.EndDate);

    public static bool IsPublicSeason(FootballSeason season) =>
        season.IsActive || season.IsCompleted || HasEnded(season.EndDate);

    public static bool IsPublicTournament(FloorballTournament tournament) =>
        tournament.TournamentStatus != FloorballTournamentStatus.Draft;

    public static bool IsPublicTournament(FootballTournament tournament) =>
        tournament.TournamentStatus != FootballTournamentStatus.Draft;

    public static bool IsPublic(HockeyCompetition competition) =>
        competition.Status != HockeyCompetitionStatus.Draft
        || (competition is HockeySeason && HasEnded(competition.EndDate));

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
