using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace JoomleagueImporter.Import;

/// <summary>
/// HTTP client against the current WebAPI controllers (api/floorball-matches routes,
/// passwordless auth with autoFillCode).
/// </summary>
public class ApiClient : IDisposable
{
    private const string DefaultAuthEmail = "test@myleague.local";

    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient();
        _http.BaseAddress = new Uri(baseUrl);
        _http.Timeout = TimeSpan.FromMinutes(2);
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _json = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        _json.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task AuthenticateAsync(string? email = null)
    {
        string loginEmail = string.IsNullOrWhiteSpace(email) ? DefaultAuthEmail : email.Trim();
        Console.WriteLine($"Authenticating as {loginEmail}...");

        HttpResponseMessage loginResp = await _http.PostAsJsonAsync("api/auth/login", new { email = loginEmail });
        await EnsureSuccess(loginResp, "Login");

        ApiResponse<LoginAutoFillResponse>? loginApi =
            await loginResp.Content.ReadFromJsonAsync<ApiResponse<LoginAutoFillResponse>>(_json);
        if (string.IsNullOrEmpty(loginApi?.Data?.AutoFillCode))
            throw new InvalidOperationException(
                "Login response contained no auto-fill code. Is the API running in Development mode with LoginCode:AutoFillLoginCode = true?");

        HttpResponseMessage verifyResp = await _http.PostAsJsonAsync("api/auth/verify",
            new { email = loginEmail, code = loginApi.Data.AutoFillCode });
        await EnsureSuccess(verifyResp, "Verify");

        ApiResponse<AuthTokenResponse>? verifyApi =
            await verifyResp.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>(_json);
        if (verifyApi?.Data?.AccessToken == null)
            throw new InvalidOperationException("Failed to get access token.");

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", verifyApi.Data.AccessToken);
        Console.WriteLine("Authenticated successfully.\n");
    }

    // â”€â”€ Clubs â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<ClubDto>> GetClubsAsync() =>
        await GetPaginatedListAsync<ClubDto>("api/clubs?Page=1&PageSize=50");

    public async Task<ClubDto?> CreateClubAsync(string name, string city, string country = "Finland")
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/clubs",
            new { name, city, country, foundingDate = "2000-01-01" });
        return await ReadDataOrNull<ClubDto>(resp, $"Create club '{name}'");
    }

    // â”€â”€ Divisions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<DivisionDto>> GetDivisionsAsync() =>
        await GetPaginatedListAsync<DivisionDto>("api/divisions?Page=1&PageSize=50");

    public async Task<DivisionDto?> CreateDivisionAsync(string name, string description, int level, string sportType = "Floorball")
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/divisions", new { name, description, level, sportType });
        return await ReadDataOrNull<DivisionDto>(resp, $"Create division '{name}'");
    }

    // â”€â”€ Persons â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<PersonDto>> SearchPersonsAsync(string name)
    {
        HttpResponseMessage resp = await _http.GetAsync(
            $"api/persons/search?name={Uri.EscapeDataString(name)}&page=1&PageSize=50");
        if (!resp.IsSuccessStatusCode) return [];
        PaginatedApiResponse<PersonDto>? api =
            await resp.Content.ReadFromJsonAsync<PaginatedApiResponse<PersonDto>>(_json);
        return api?.Data?.ToList() ?? [];
    }

    public async Task<PersonDto?> CreatePersonAsync(string firstName, string lastName, DateTime? birthDate = null)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/persons", new
        {
            firstName,
            lastName,
            birthDate = birthDate?.ToString("yyyy-MM-dd"),
            isRegistered = false,
        });
        return await ReadDataOrNull<PersonDto>(resp, $"Create person '{firstName} {lastName}'");
    }

    // â”€â”€ Floorball players â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<FloorballPlayerDto>> GetPlayersAsync() =>
        await GetPaginatedListAsync<FloorballPlayerDto>("api/floorballplayer?Page=1&PageSize=50&IsActive=");

    public async Task<FloorballPlayerDto?> CreatePlayerAsync(Guid personId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballplayer", new { personId });
        return await ReadDataOrNull<FloorballPlayerDto>(resp, $"Create player for person {personId}");
    }

    // â”€â”€ Floorball teams â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<FloorballTeamDto>> GetTeamsAsync() =>
        await GetPaginatedListAsync<FloorballTeamDto>("api/floorballteam?Page=1&PageSize=50");

    public async Task<FloorballTeamDto?> CreateTeamAsync(string name, string shortName, Guid clubId, Guid? divisionId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballteam", new
        {
            name,
            clubId,
            divisionId,
            homeArena = "MAHL Arena",
            primaryJerseyColor = "",
            teamCategory = "Adult",
            shortName,
        });
        return await ReadDataOrNull<FloorballTeamDto>(resp, $"Create team '{name}'");
    }

    public async Task<bool> AddPlayerToTeamAsync(Guid teamId, Guid playerId, int position, int? jerseyNumber)
    {
        string url = $"api/floorballteam/{teamId}/players/{playerId}?position={position}";
        if (jerseyNumber.HasValue)
            url += $"&jerseyNumber={jerseyNumber.Value}";

        HttpResponseMessage resp = await _http.PostAsync(url, null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            if (body.Contains("is already in the roster", StringComparison.OrdinalIgnoreCase)) return true;

            // Old data contains duplicate jersey numbers within a team; retry without a number
            // instead of silently dropping the player from the roster.
            if (jerseyNumber.HasValue &&
                body.Contains("Jersey number", StringComparison.OrdinalIgnoreCase))
            {
                return await AddPlayerToTeamAsync(teamId, playerId, position, null);
            }

            Console.WriteLine($"  WARN: Add player to team failed: {Truncate(body)}");
            return false;
        }
        return true;
    }

    // â”€â”€ Referees â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<FloorballRefereeDto>> GetRefereesAsync() =>
        await GetPaginatedListAsync<FloorballRefereeDto>("api/floorballreferee?page=1&PageSize=50");

    public async Task<FloorballRefereeDto?> CreateRefereeAsync(Guid personId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballreferee", new
        {
            personId,
            licenseIssueDate = "2020-01-01",
            licenseExpiryDate = "2035-12-31",
        });
        return await ReadDataOrNull<FloorballRefereeDto>(resp, $"Create referee for person {personId}");
    }

    // â”€â”€ Seasons â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<FloorballSeasonDto>> GetSeasonsAsync() =>
        await GetPaginatedListAsync<FloorballSeasonDto>("api/floorballseason?Page=1&PageSize=50");

    public async Task<FloorballSeasonDto?> CreateSeasonAsync(
        string name,
        Guid divisionId,
        DateTime startDate,
        DateTime endDate,
        int numberOfPeriods,
        int periodDurationMinutes)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballseason", new
        {
            name,
            divisionIds = new[] { divisionId },
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd"),
            numberOfPeriods,
            periodDurationMinutes,
            allowOvertime = false,
            allowShootout = false,
        });
        return await ReadDataOrNull<FloorballSeasonDto>(resp, $"Create season '{name}'");
    }

    public async Task<bool> AddTeamToSeasonAsync(Guid seasonId, Guid teamId)
    {
        HttpResponseMessage resp = await _http.PostAsync($"api/floorballseason/{seasonId}/teams/{teamId}", null);
        return await OkOrAlready(resp, "AddTeamToSeason");
    }

    public async Task<bool> AddTeamToSeasonDivisionAsync(Guid seasonId, Guid divisionId, Guid teamId)
    {
        HttpResponseMessage resp = await _http.PostAsync(
            $"api/floorballseason/{seasonId}/divisions/{divisionId}/teams/{teamId}", null);
        return await OkOrAlready(resp, "AddTeamToSeasonDivision");
    }

    // â”€â”€ Matches â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<FloorballMatchDto?> CreateMatchAsync(
        Guid competitionId,
        Guid homeTeamId,
        Guid awayTeamId,
        Guid refereeId,
        DateTime scheduledDateTime,
        string? venue)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorball-matches", new
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
        HttpResponseMessage resp = await _http.PutAsync(
            $"api/floorball-matches/{matchId}/teams/{teamId}/goalie/{goaliePlayerId}", null);
        return await OkOrWarn(resp, "SetGoalie");
    }

    public async Task<bool> StartMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorball-matches/{matchId}/start", null);
        return await OkOrWarn(resp, "StartMatch");
    }

    public async Task<bool> StartPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await _http.PostAsync(
            $"api/floorball-matches/{matchId}/events/periods/{periodNumber}/start", null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> EndPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await _http.PostAsync(
            $"api/floorball-matches/{matchId}/events/periods/{periodNumber}/end", null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> RecordGoalAsync(
        Guid matchId,
        Guid scoringTeamId,
        Guid scoringPlayerId,
        Guid? assistingPlayerId,
        Guid? secondaryAssistingPlayerId,
        int periodNumber,
        int timeInSeconds)
    {
        object request = new
        {
            matchId,
            scoringTeamId,
            scoringPlayerId,
            assistingPlayerId,
            secondaryAssistingPlayerIs = secondaryAssistingPlayerId,
            periodNumber,
            timeInSeconds,
        };
        return await PostWithRetryAsync($"api/floorball-matches/{matchId}/events/goal", request, "RecordGoal");
    }

    public async Task<bool> RecordPenaltyAsync(
        Guid matchId,
        Guid teamId,
        Guid playerId,
        int durationMinutes,
        int periodNumber,
        int timeInSeconds,
        string penaltyType = "Minor")
    {
        object request = new
        {
            matchId,
            teamId,
            playerId,
            durationMinutes,
            periodNumber,
            timeInSeconds,
            penaltyType,
        };
        return await PostWithRetryAsync($"api/floorball-matches/{matchId}/events/penalty", request, "RecordPenalty");
    }

    public async Task<bool> CompleteMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorball-matches/{matchId}/complete", null);
        return await OkOrWarn(resp, "CompleteMatch");
    }

    public async Task<FloorballMatchDto?> GetMatchByIdAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.GetAsync($"api/floorball-matches/by-id/{matchId}");
        return await ReadDataOrNull<FloorballMatchDto>(resp, $"Get match {matchId}");
    }

    public async Task<bool> ReopenMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorball-matches/{matchId}/reopen", null);
        return await OkOrWarn(resp, "ReopenMatch");
    }

    public async Task<bool> DeleteGoalEventAsync(Guid matchId, Guid goalEventId)
    {
        HttpResponseMessage resp = await _http.DeleteAsync($"api/floorball-matches/{matchId}/events/goal/{goalEventId}");
        return await OkOrWarn(resp, "DeleteGoalEvent");
    }

    public async Task<bool> DeletePenaltyEventAsync(Guid matchId, Guid penaltyEventId)
    {
        HttpResponseMessage resp = await _http.DeleteAsync($"api/floorball-matches/{matchId}/events/penalty/{penaltyEventId}");
        return await OkOrWarn(resp, "DeletePenaltyEvent");
    }

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private async Task<bool> PostWithRetryAsync(string url, object payload, string operationName, int maxRetries = 5)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            HttpResponseMessage resp = await _http.PostAsJsonAsync(url, payload);
            if (resp.IsSuccessStatusCode)
                return true;

            if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < maxRetries)
            {
                await Task.Delay(150);
                continue;
            }

            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"    WARN: {operationName} failed: {Truncate(body)}");
            return false;
        }
        return false;
    }

    private async Task<List<T>> GetPaginatedListAsync<T>(string url)
    {
        List<T> all = [];
        string currentUrl = url;

        while (true)
        {
            HttpResponseMessage resp = await _http.GetAsync(currentUrl);
            if (!resp.IsSuccessStatusCode) break;

            PaginatedApiResponse<T>? api = await resp.Content.ReadFromJsonAsync<PaginatedApiResponse<T>>(_json);
            if (api?.Data != null)
                all.AddRange(api.Data);

            if (api?.Pagination.HasNextPage != true) break;

            int nextPage = api.Pagination.CurrentPage + 1;
            currentUrl = Regex.Replace(url, @"Page=\d+", $"Page={nextPage}", RegexOptions.IgnoreCase);
        }

        return all;
    }

    private async Task<T?> ReadDataOrNull<T>(HttpResponseMessage resp, string operation) where T : class
    {
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: {operation} failed ({(int)resp.StatusCode}): {Truncate(body)}");
            return null;
        }
        ApiResponse<T>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(_json);
        return api?.Data;
    }

    private static async Task<bool> OkOrWarn(HttpResponseMessage resp, string operation)
    {
        if (resp.IsSuccessStatusCode) return true;
        string body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"  WARN: {operation} failed: {Truncate(body)}");
        return false;
    }

    private static async Task<bool> OkOrAlready(HttpResponseMessage resp, string operation)
    {
        if (resp.IsSuccessStatusCode) return true;
        string body = await resp.Content.ReadAsStringAsync();
        if (body.Contains("already", StringComparison.OrdinalIgnoreCase)) return true;
        Console.WriteLine($"  WARN: {operation} failed: {Truncate(body)}");
        return false;
    }

    private static string Truncate(string s) => s.Length <= 300 ? s : s[..300] + "...";

    private static async Task EnsureSuccess(HttpResponseMessage resp, string operation)
    {
        if (resp.IsSuccessStatusCode) return;
        string body = await resp.Content.ReadAsStringAsync();
        throw new HttpRequestException($"{operation} failed ({(int)resp.StatusCode}): {body}");
    }

    public void Dispose() => _http.Dispose();

    private class LoginAutoFillResponse
    {
        public string? AutoFillCode { get; set; }
    }

    private class AuthTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
