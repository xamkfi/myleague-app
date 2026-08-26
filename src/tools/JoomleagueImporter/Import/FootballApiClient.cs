using System.Net.Http.Json;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Teams.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Football;

namespace JoomleagueImporter.Import;

/// <summary>
/// Football WebAPI routes used by the JoomLeague importer.
/// </summary>
public class FootballApiClient : ImportApiClient
{
    public FootballApiClient(string baseUrl) : base(baseUrl)
    {
    }

    public async Task<List<FootballPlayerDto>> GetPlayersAsync() =>
        await GetPaginatedListAsync<FootballPlayerDto>("api/FootballPlayer?Page=1&PageSize=50&IsActive=");

    public async Task<FootballPlayerDto?> CreatePlayerAsync(Guid personId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/FootballPlayer", new { personId });
        return await ReadDataOrNull<FootballPlayerDto>(resp, $"Create football player for person {personId}");
    }

    public async Task<List<Application.Features.Football.Teams.DTOs.FootballTeamSummaryDto>> GetTeamsAsync() =>
        await GetPaginatedListAsync<Application.Features.Football.Teams.DTOs.FootballTeamSummaryDto>(
            "api/FootballTeam/without-roster?Page=1&PageSize=50");

    public async Task<FootballTeamDto?> CreateTeamAsync(
        string name,
        string shortName,
        Guid clubId,
        Guid? divisionId,
        TeamCategory teamCategory = TeamCategory.Adult)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/FootballTeam", new
        {
            name,
            clubId,
            divisionId,
            homeArena = "MAHL Arena",
            primaryJerseyColor = "",
            category = teamCategory.ToString(),
            shortName,
        });
        return await ReadDataOrNull<FootballTeamDto>(resp, $"Create football team '{name}'");
    }

    public Task<bool> AddPlayerToTeamAsync(Guid teamId, Guid playerId, int position, int? jerseyNumber) =>
        AddPlayerToTeamByQueryAsync(
            $"api/FootballTeam/{teamId}/players/{playerId}?position={position}",
            jerseyNumber,
            "Add football player to team");

    public async Task<List<FootballRefereeDto>> GetRefereesAsync() =>
        await GetPaginatedListAsync<FootballRefereeDto>("api/FootballReferee?page=1&PageSize=50");

    public async Task<FootballRefereeDto?> CreateRefereeAsync(Guid personId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/FootballReferee", new
        {
            personId,
            licenseIssueDate = "2020-01-01",
            licenseExpiryDate = "2035-12-31",
        });
        return await ReadDataOrNull<FootballRefereeDto>(resp, $"Create football referee for person {personId}");
    }

    public async Task<List<FootballSeasonDto>> GetSeasonsAsync() =>
        await GetUnpaginatedListAsync<FootballSeasonDto>("api/FootballSeason");

    public async Task<FootballSeasonDto?> CreateSeasonAsync(
        string name,
        Guid divisionId,
        DateTime startDate,
        DateTime endDate,
        int numberOfHalves,
        int halfDurationMinutes,
        int playersOnField,
        TeamCategory teamCategory = TeamCategory.Adult)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/FootballSeason", new
        {
            name,
            divisionIds = new[] { divisionId },
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd"),
            numberOfHalves,
            halfDurationMinutes,
            playersOnField,
            requireGoalkeeper = true,
            requireOfficialsToStart = false,
            allowExtraTime = false,
            allowPenaltyShootout = false,
            teamCategory = teamCategory.ToString(),
        });
        return await ReadDataOrNull<FootballSeasonDto>(resp, $"Create football season '{name}'");
    }

    public async Task<FootballSeasonDto?> UpdateSeasonAsync(
        FootballSeasonDto season,
        TeamCategory teamCategory)
    {
        HttpResponseMessage resp = await Http.PutAsJsonAsync($"api/FootballSeason/{season.Id}", new
        {
            name = season.Name,
            startDate = season.StartDate.ToString("yyyy-MM-dd"),
            endDate = season.EndDate.ToString("yyyy-MM-dd"),
            numberOfHalves = season.MatchRules.NumberOfHalves,
            halfDurationMinutes = season.MatchRules.HalfDurationMinutes,
            playersOnField = season.MatchRules.PlayersOnField,
            requireGoalkeeper = season.MatchRules.RequireGoalkeeper,
            maxSubstitutions = season.MatchRules.MaxSubstitutions,
            requireOfficialsToStart = season.MatchRules.RequireOfficialsToStart,
            allowExtraTime = season.MatchRules.AllowExtraTime,
            extraTimeHalfCount = season.MatchRules.ExtraTimeHalfCount,
            extraTimeHalfDurationMinutes = season.MatchRules.ExtraTimeHalfDurationMinutes,
            allowPenaltyShootout = season.MatchRules.AllowPenaltyShootout,
            teamCategory = teamCategory.ToString(),
        });
        return await ReadDataOrNull<FootballSeasonDto>(resp, $"Update football season '{season.Name}' category");
    }

    public async Task<bool> AddTeamToSeasonAsync(Guid seasonId, Guid teamId)
    {
        HttpResponseMessage resp = await Http.PostAsync($"api/FootballSeason/{seasonId}/teams/{teamId}", null);
        return await OkOrAlready(resp, "AddTeamToFootballSeason");
    }

    public async Task<bool> AddTeamToSeasonDivisionAsync(Guid seasonId, Guid divisionId, Guid teamId)
    {
        HttpResponseMessage resp = await Http.PostAsync(
            $"api/FootballSeason/{seasonId}/divisions/{divisionId}/teams/{teamId}", null);
        return await OkOrAlready(resp, "AddTeamToFootballSeasonDivision");
    }

    public async Task<FootballMatchDto?> CreateMatchAsync(
        Guid competitionId,
        Guid homeTeamId,
        Guid awayTeamId,
        Guid refereeId,
        DateTime scheduledDateTime,
        string? venue)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/football-matches", new
        {
            competitionId,
            homeTeamId,
            awayTeamId,
            refereeId,
            scheduledDateTime = scheduledDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
            venue,
        });
        return await ReadDataOrNull<FootballMatchDto>(resp, "Create football match");
    }

    public async Task<bool> SetLineupAsync(
        Guid matchId,
        Guid teamId,
        IReadOnlyList<(Guid PlayerId, FootballPosition Position, bool IsOnField)> players)
    {
        object request = new
        {
            players = players.Select(p => new
            {
                playerId = p.PlayerId,
                position = p.Position.ToString(),
                isOnField = p.IsOnField,
            }),
        };
        HttpResponseMessage resp = await Http.PutAsJsonAsync(
            $"api/football-matches/{matchId}/teams/{teamId}/lineup", request);
        return await OkOrWarn(resp, "SetFootballLineup");
    }

    public async Task<bool> StartMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.PutAsync($"api/football-matches/{matchId}/start", null);
        return await OkOrWarn(resp, "StartFootballMatch");
    }

    public async Task<bool> StartPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await Http.PostAsync(
            $"api/football-matches/{matchId}/events/periods/{periodNumber}/start", null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> EndPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await Http.PostAsync(
            $"api/football-matches/{matchId}/events/periods/{periodNumber}/end", null);
        return resp.IsSuccessStatusCode;
    }

    public Task<bool> RecordGoalAsync(
        Guid matchId,
        Guid scoringTeamId,
        Guid scoringPlayerId,
        Guid? assistingPlayerId,
        int periodNumber,
        int timeInSeconds) =>
        PostWithRetryAsync($"api/football-matches/{matchId}/events/goal", new
        {
            matchId,
            scoringTeamId,
            scoringPlayerId,
            assistingPlayerId,
            periodNumber,
            timeInSeconds,
        }, "RecordFootballGoal");

    public Task<bool> RecordCardAsync(
        Guid matchId,
        Guid teamId,
        Guid playerId,
        FootballCardType cardType,
        int periodNumber,
        int timeInSeconds) =>
        PostWithRetryAsync($"api/football-matches/{matchId}/events/card", new
        {
            matchId,
            teamId,
            playerId,
            cardType = cardType.ToString(),
            periodNumber,
            timeInSeconds,
        }, "RecordFootballCard");

    public async Task<bool> CompleteMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.PutAsync($"api/football-matches/{matchId}/complete", null);
        return await OkOrWarn(resp, "CompleteFootballMatch");
    }

    public async Task<FootballMatchDto?> GetMatchByIdAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.GetAsync($"api/football-matches/by-id/{matchId}");
        return await ReadDataOrNull<FootballMatchDto>(resp, $"Get football match {matchId}");
    }

    public async Task<bool> ReopenMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.PutAsync($"api/football-matches/{matchId}/reopen", null);
        return await OkOrWarn(resp, "ReopenFootballMatch");
    }

    public async Task<bool> DeleteGoalEventAsync(Guid matchId, Guid goalEventId)
    {
        HttpResponseMessage resp = await Http.DeleteAsync($"api/football-matches/{matchId}/events/goal/{goalEventId}");
        return await OkOrWarn(resp, "DeleteFootballGoalEvent");
    }

    public async Task<bool> DeleteCardEventAsync(Guid matchId, Guid cardEventId)
    {
        HttpResponseMessage resp = await Http.DeleteAsync($"api/football-matches/{matchId}/events/card/{cardEventId}");
        return await OkOrWarn(resp, "DeleteFootballCardEvent");
    }
}
