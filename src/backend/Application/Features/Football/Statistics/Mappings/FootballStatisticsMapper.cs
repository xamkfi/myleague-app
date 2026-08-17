using Application.Features.Common.Persons.Mappings;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Teams.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;

namespace Application.Features.Football.Statistics.Mappings;

/// <summary>
/// Mapper for converting football statistics entities to DTOs
/// </summary>
public static class FootballStatisticsMapper
{
    /// <summary>
    /// Sentinel competition ID embedded into aggregated DTOs. We pick <see cref="Guid.Empty"/>
    /// instead of one of the source competition IDs because the aggregated row is synthetic and
    /// must not be mistaken for a real competition by the frontend.
    /// </summary>
    public static readonly Guid AggregatedCompetitionId = Guid.Empty;

    /// <summary>
    /// Converts FootballTeamSeasonStatistics entity to DTO
    /// </summary>
    public static FootballTeamSeasonStatisticsDto ToDto(FootballTeamSeasonStatistics entity)
    {
        return new FootballTeamSeasonStatisticsDto
        {
            Id = entity.Id,
            TeamId = entity.TeamId,
            CompetitionId = entity.CompetitionId,
            TeamName = entity.Team?.Name ?? string.Empty,
            TeamLogo = entity.Team?.LogoUrl,
            SeasonName = entity.Competition?.Name ?? string.Empty,
            GamesPlayed = entity.GamesPlayed,
            Wins = entity.Wins,
            Losses = entity.Losses,
            Draws = entity.Draws,
            Points = entity.Points,
            GoalsFor = entity.GoalsFor,
            GoalsAgainst = entity.GoalsAgainst,
            GoalDifference = entity.GoalDifference,
            HomeWins = entity.HomeWins,
            HomeLosses = entity.HomeLosses,
            AwayWins = entity.AwayWins,
            AwayLosses = entity.AwayLosses,
            CleanSheets = entity.CleanSheets,
            YellowCards = entity.YellowCards,
            RedCards = entity.RedCards,
            LastFiveForm = Array.Empty<FootballGameResult>()
        };
    }

    /// <summary>
    /// Aggregates a team's per-competition statistics rows into a single career-style DTO.
    /// </summary>
    public static FootballTeamSeasonStatisticsDto AggregateTeamStatistics(
        Guid teamId,
        IReadOnlyCollection<FootballTeamSeasonStatistics> rows,
        string teamName,
        Uri? teamLogo,
        string? seasonName = null)
    {
        FootballTeamSeasonStatisticsDto dto = new FootballTeamSeasonStatisticsDto
        {
            Id = Guid.Empty,
            TeamId = teamId,
            CompetitionId = AggregatedCompetitionId,
            TeamName = teamName,
            TeamLogo = teamLogo,
            SeasonName = seasonName ?? string.Empty,
            LastFiveForm = Array.Empty<FootballGameResult>()
        };

        foreach (FootballTeamSeasonStatistics row in rows)
        {
            dto.GamesPlayed += row.GamesPlayed;
            dto.Wins += row.Wins;
            dto.Losses += row.Losses;
            dto.Draws += row.Draws;
            dto.Points += row.Points;
            dto.GoalsFor += row.GoalsFor;
            dto.GoalsAgainst += row.GoalsAgainst;
            dto.HomeWins += row.HomeWins;
            dto.HomeLosses += row.HomeLosses;
            dto.AwayWins += row.AwayWins;
            dto.AwayLosses += row.AwayLosses;
            dto.CleanSheets += row.CleanSheets;
            dto.YellowCards += row.YellowCards;
            dto.RedCards += row.RedCards;
        }

        dto.GoalDifference = dto.GoalsFor - dto.GoalsAgainst;
        return dto;
    }

    /// <summary>
    /// Aggregates per-competition stats rows for a single player on a single team into one DTO.
    /// </summary>
    public static FootballPlayerSeasonStatisticsDto AggregatePlayerStatistics(
        Guid playerId,
        Guid teamId,
        string playerName,
        string teamName,
        string? teamLogo,
        IReadOnlyCollection<FootballPlayerSeasonStatistics> rows)
    {
        FootballPlayerSeasonStatisticsDto dto = new FootballPlayerSeasonStatisticsDto
        {
            Id = Guid.Empty,
            PlayerId = playerId,
            TeamId = teamId,
            CompetitionId = AggregatedCompetitionId,
            PlayerName = playerName,
            TeamName = teamName,
            TeamLogo = teamLogo,
            SeasonName = string.Empty
        };

        foreach (FootballPlayerSeasonStatistics row in rows)
        {
            dto.GamesPlayed += row.GamesPlayed;
            dto.Goals += row.Goals;
            dto.Assists += row.Assists;
            dto.Points += row.Points;
            dto.YellowCards += row.YellowCards;
            dto.RedCards += row.RedCards;
        }

        return dto;
    }

    /// <summary>
    /// Converts FootballPlayerSeasonStatistics entity to DTO
    /// </summary>
    public static FootballPlayerSeasonStatisticsDto ToDto(FootballPlayerSeasonStatistics entity, string? playerName = null)
    {
        return new FootballPlayerSeasonStatisticsDto
        {
            Id = entity.Id,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            CompetitionId = entity.CompetitionId,
            PlayerName = playerName ?? string.Empty,
            TeamName = entity.Team?.Name ?? string.Empty,
            SeasonName = entity.Competition?.Name ?? string.Empty,
            TeamLogo = entity.Team?.LogoUrl?.ToString(),
            GamesPlayed = entity.GamesPlayed,
            Goals = entity.Goals,
            Assists = entity.Assists,
            Points = entity.Points,
            YellowCards = entity.YellowCards,
            RedCards = entity.RedCards
        };
    }

    /// <summary>
    /// Converts FootballMatchTeamStatistics entity to DTO
    /// </summary>
    public static FootballMatchTeamStatisticsDto ToDto(FootballMatchTeamStatistics entity, string? teamName = null)
    {
        return new FootballMatchTeamStatisticsDto
        {
            Id = entity.Id,
            MatchId = entity.MatchId,
            TeamId = entity.TeamId,
            TeamName = teamName ?? entity.Team?.Name ?? string.Empty,
            Goals = entity.Goals,
            YellowCards = entity.YellowCards,
            RedCards = entity.RedCards,
            Substitutions = entity.Substitutions,
            CleanSheet = entity.CleanSheet
        };
    }

    public static FootballPlayerProfileDto ToDto(FootballPlayer player, Person person, List<FootballPlayerSeasonStatistics> statistics)
    {
        return new FootballPlayerProfileDto
        {
            Player = new FootballPlayerPublicDto(
                player.Id,
                player.PersonId,
                PersonMapper.ToPublicDto(person),
                player.IsActive,
                player.Position.PrimaryPosition,
                player.CareerGoals,
                player.CareerAssists,
                null
            ),
            SeasonStatistics = statistics.Select(stat => new FootballPlayerSeasonStatisticsDto
            {
                Id = stat.Id,
                PlayerId = stat.PlayerId,
                TeamId = stat.TeamId,
                CompetitionId = stat.CompetitionId,
                PlayerName = person.FullName,
                TeamName = stat.Team?.Name ?? string.Empty,
                TeamLogo = stat.Team?.LogoUrl?.ToString(),
                SeasonName = stat.Competition?.Name ?? string.Empty,
                GamesPlayed = stat.GamesPlayed,
                Goals = stat.Goals,
                Assists = stat.Assists,
                Points = stat.Points,
                YellowCards = stat.YellowCards,
                RedCards = stat.RedCards
            }).ToList()
        };
    }
}
