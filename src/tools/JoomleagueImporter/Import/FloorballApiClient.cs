using System.Net.Http.Json;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Enums.Common;

namespace JoomleagueImporter.Import;

/// <summary>
/// Floorball WebAPI routes used by the JoomLeague importer.
/// </summary>
public class FloorballApiClient : ImportApiClient
{
    public FloorballApiClient(string baseUrl) : base(baseUrl)
    {
    }

    public async Task<List<FloorballPlayerDto>> GetPlayersAsync() =>
        await GetPaginatedListAsync<FloorballPlayerDto>("api/floorballplayer?Page=1&PageSize=50&IsActive=");

    public async Task<FloorballPlayerDto?> CreatePlayerAsync(Guid personId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/floorballplayer", new { personId });
        return await ReadDataOrNull<FloorballPlayerDto>(resp, $"Create player for person {personId}");
    }

    public async Task<List<FloorballTeamDto>> GetTeamsAsync() =>
        await GetPaginatedListAsync<FloorballTeamDto>("api/floorballteam?Page=1&PageSize=50");

    public async Task<FloorballTeamDto?> CreateTeamAsync(
        string name,
        string shortName,
        Guid clubId,
        Guid? divisionId,
        TeamCategory teamCategory = TeamCategory.Adult)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/floorballteam", new
        {
            name,
            clubId,
            divisionId,
            homeArena = "MAHL Arena",
            primaryJerseyColor = "",
            category = teamCategory.ToString(),
            shortName,
        });
        return await ReadDataOrNull<FloorballTeamDto>(resp, $"Create team '{name}'");
    }

    public Task<bool> AddPlayerToTeamAsync(Guid teamId, Guid playerId, int position, int? jerseyNumber) =>
        AddPlayerToTeamByQueryAsync(
            $"api/floorballteam/{teamId}/players/{playerId}?position={position}",
            jerseyNumber,
            "Add player to team");

    public async Task<List<FloorballRefereeDto>> GetRefereesAsync() =>
        await GetPaginatedListAsync<FloorballRefereeDto>("api/floorballreferee?page=1&PageSize=50");

    public async Task<FloorballRefereeDto?> CreateRefereeAsync(Guid personId)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/floorballreferee", new
        {
            personId,
            licenseIssueDate = "2020-01-01",
            licenseExpiryDate = "2035-12-31",
        });
        return await ReadDataOrNull<FloorballRefereeDto>(resp, $"Create referee for person {personId}");
    }

    public async Task<List<FloorballSeasonDto>> GetSeasonsAsync() =>
        await GetPaginatedListAsync<FloorballSeasonDto>("api/floorballseason?Page=1&PageSize=50");

    public async Task<FloorballSeasonDto?> CreateSeasonAsync(
        string name,
        Guid divisionId,
        DateTime startDate,
        DateTime endDate,
        int numberOfPeriods,
        int periodDurationMinutes,
        TeamCategory teamCategory = TeamCategory.Adult)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/floorballseason", new
        {
            name,
            divisionIds = new[] { divisionId },
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd"),
            numberOfPeriods,
            periodDurationMinutes,
            allowOvertime = false,
            allowShootout = false,
            teamCategory = teamCategory.ToString(),
        });
        return await ReadDataOrNull<FloorballSeasonDto>(resp, $"Create season '{name}'");
    }

    public async Task<FloorballSeasonDto?> UpdateSeasonAsync(
        FloorballSeasonDto season,
        TeamCategory teamCategory)
    {
        HttpResponseMessage resp = await Http.PutAsJsonAsync($"api/floorballseason/{season.Id}", new
        {
            name = season.Name,
            startDate = season.StartDate.ToString("yyyy-MM-dd"),
            endDate = season.EndDate.ToString("yyyy-MM-dd"),
            numberOfPeriods = season.MatchRules.NumberOfPeriods,
            periodDurationMinutes = season.MatchRules.PeriodDurationMinutes,
            allowOvertime = season.MatchRules.AllowOvertime,
            overtimeDurationMinutes = season.MatchRules.OvertimeDurationMinutes,
            allowShootout = season.MatchRules.AllowShootout,
            teamCategory = teamCategory.ToString(),
        });
        return await ReadDataOrNull<FloorballSeasonDto>(resp, $"Update season '{season.Name}' category");
    }

    public async Task<bool> AddTeamToSeasonAsync(Guid seasonId, Guid teamId)
    {
        HttpResponseMessage resp = await Http.PostAsync($"api/floorballseason/{seasonId}/teams/{teamId}", null);
        return await OkOrAlready(resp, "AddTeamToSeason");
    }

    public async Task<bool> AddTeamToSeasonDivisionAsync(Guid seasonId, Guid divisionId, Guid teamId)
    {
        HttpResponseMessage resp = await Http.PostAsync(
            $"api/floorballseason/{seasonId}/divisions/{divisionId}/teams/{teamId}", null);
        return await OkOrAlready(resp, "AddTeamToSeasonDivision");
    }

    public async Task<FloorballMatchDto?> CreateMatchAsync(
        Guid competitionId,
        Guid homeTeamId,
        Guid awayTeamId,
        Guid refereeId,
        DateTime scheduledDateTime,
        string? venue)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/floorball-matches", new
        {
            competitionId,
            homeTeamId,
            awayTeamId,
            refereeId,
            scheduledDateTime = scheduledDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
            venue,
        });
        return await ReadDataOrNull<FloorballMatchDto>(resp, "Create match");
    }

    public async Task<bool> SetGoalieAsync(Guid matchId, Guid teamId, Guid goaliePlayerId)
    {
        HttpResponseMessage resp = await Http.PutAsync(
            $"api/floorball-matches/{matchId}/teams/{teamId}/goalie/{goaliePlayerId}", null);
        return await OkOrWarn(resp, "SetGoalie");
    }

    public async Task<bool> StartMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.PutAsync($"api/floorball-matches/{matchId}/start", null);
        return await OkOrWarn(resp, "StartMatch");
    }

    public async Task<bool> StartPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await Http.PostAsync(
            $"api/floorball-matches/{matchId}/events/periods/{periodNumber}/start", null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> EndPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await Http.PostAsync(
            $"api/floorball-matches/{matchId}/events/periods/{periodNumber}/end", null);
        return resp.IsSuccessStatusCode;
    }

    public Task<bool> RecordGoalAsync(
        Guid matchId,
        Guid scoringTeamId,
        Guid scoringPlayerId,
        Guid? assistingPlayerId,
        Guid? secondaryAssistingPlayerId,
        int periodNumber,
        int timeInSeconds) =>
        PostWithRetryAsync($"api/floorball-matches/{matchId}/events/goal", new
        {
            matchId,
            scoringTeamId,
            scoringPlayerId,
            assistingPlayerId,
            secondaryAssistingPlayerIs = secondaryAssistingPlayerId,
            periodNumber,
            timeInSeconds,
            skipRateLimit = true,
        }, "RecordGoal");

    public Task<FloorballMatchEventsImportDto?> ImportEventsAsync(
        Guid matchId,
        IReadOnlyList<object> events) =>
        PostDataOrNullAsync<FloorballMatchEventsImportDto>(
            $"api/floorball-matches/{matchId}/events/import",
            new { events },
            "ImportMatchEvents",
            maxRetries: 2);

    public Task<bool> RecordPenaltyAsync(
        Guid matchId,
        Guid teamId,
        Guid playerId,
        int durationMinutes,
        int periodNumber,
        int timeInSeconds,
        string penaltyType = "Minor") =>
        PostWithRetryAsync($"api/floorball-matches/{matchId}/events/penalty", new
        {
            matchId,
            teamId,
            playerId,
            durationMinutes,
            periodNumber,
            timeInSeconds,
            penaltyType,
        }, "RecordPenalty");

    public async Task<bool> CompleteMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.PutAsync($"api/floorball-matches/{matchId}/complete", null);
        return await OkOrWarn(resp, "CompleteMatch");
    }

    public async Task<FloorballMatchDto?> GetMatchByIdAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.GetAsync($"api/floorball-matches/by-id/{matchId}");
        return await ReadDataOrNull<FloorballMatchDto>(resp, $"Get match {matchId}");
    }

    public async Task<bool> ReopenMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await Http.PutAsync($"api/floorball-matches/{matchId}/reopen", null);
        return await OkOrWarn(resp, "ReopenMatch");
    }

    public async Task<bool> DeleteGoalEventAsync(Guid matchId, Guid goalEventId)
    {
        HttpResponseMessage resp = await Http.DeleteAsync($"api/floorball-matches/{matchId}/events/goal/{goalEventId}");
        return await OkOrWarn(resp, "DeleteGoalEvent");
    }

    public async Task<bool> DeletePenaltyEventAsync(Guid matchId, Guid penaltyEventId)
    {
        HttpResponseMessage resp = await Http.DeleteAsync($"api/floorball-matches/{matchId}/events/penalty/{penaltyEventId}");
        return await OkOrWarn(resp, "DeletePenaltyEvent");
    }
}
