using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Entities.Hockey.Competitions;

namespace Application.Features.Hockey.Competitions.Mappings;

/// <summary>
/// Maps hockey competition domain entities to application DTOs.
/// </summary>
public static class HockeyCompetitionMapper
{
    /// <summary>
    /// Maps a competition-team membership to a DTO.
    /// </summary>
    public static HockeyCompetitionTeamDto ToTeamDto(HockeyCompetitionTeam team)
    {
        return new HockeyCompetitionTeamDto(
            team.Id,
            team.CompetitionId,
            team.TeamId,
            team.Seed,
            team.JoinedAt,
            team.IsActive);
    }

    /// <summary>
    /// Maps a hockey season to a DTO.
    /// </summary>
    public static HockeySeasonDto ToSeasonDto(HockeySeason season)
    {
        return new HockeySeasonDto(
            season.Id,
            season.Name,
            season.StartDate,
            season.EndDate,
            season.Status.ToString(),
            season.IsActive,
            season.IsCompleted,
            season.SeasonCode,
            season.ChampionCompetitionTeamId,
            season.Teams.Select(ToTeamDto).ToList());
    }

    /// <summary>
    /// Maps a hockey tournament to a DTO.
    /// </summary>
    public static HockeyTournamentDto ToTournamentDto(HockeyTournament tournament)
    {
        return new HockeyTournamentDto(
            tournament.Id,
            tournament.Name,
            tournament.StartDate,
            tournament.EndDate,
            tournament.Status.ToString(),
            tournament.IsActive,
            tournament.IsCompleted,
            tournament.Venue,
            tournament.ContentHtml,
            tournament.CurrentStage.ToString(),
            tournament.ChampionCompetitionTeamId,
            tournament.Teams.Select(ToTeamDto).ToList());
    }
}
