using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace MahlImporter.Import;

public class ApiClient : IDisposable
{
    private const string DefaultAuthEmail = "test@myleague.local";

    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient();
        _http.BaseAddress = new Uri(baseUrl);
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _json = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        _json.Converters.Add(new JsonStringEnumConverter());
    }

    /// <param name="email">Email to use for login. If null, uses default (test@myleague.local).</param>
    public async Task AuthenticateAsync(string? email = null)
    {
        string loginEmail = string.IsNullOrWhiteSpace(email) ? DefaultAuthEmail : email.Trim();
        Console.WriteLine($"Authenticating as {loginEmail}...");

        HttpResponseMessage loginResp = await _http.PostAsJsonAsync("api/auth/login", new { email = loginEmail });
        await EnsureSuccess(loginResp, "Login");

        ApiResponse<LoginDevResponse>? loginApi = await loginResp.Content.ReadFromJsonAsync<ApiResponse<LoginDevResponse>>(_json);
        if (loginApi?.Data?.DevCode == null)
            throw new InvalidOperationException("Failed to get dev login code. Is the API running in Development mode?");

        HttpResponseMessage verifyResp = await _http.PostAsJsonAsync("api/auth/verify", new { email = loginEmail, code = loginApi.Data.DevCode });
        await EnsureSuccess(verifyResp, "Verify");

        ApiResponse<AuthTokenResponse>? verifyApi = await verifyResp.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>(_json);
        if (verifyApi?.Data?.AccessToken == null)
            throw new InvalidOperationException("Failed to get access token.");

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", verifyApi.Data.AccessToken);
        Console.WriteLine("Authenticated successfully.\n");
    }

    // ── Clubs ─────────────────────────────────────────────

    public async Task<List<ClubDto>> GetClubsAsync()
    {
        return await GetPaginatedListAsync<ClubDto>("api/clubs?Page=1&PageSize=100");
    }

    public async Task<ClubDto?> CreateClubAsync(string name, string city = "Mikkeli", string country = "Finland")
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/clubs", new { name, city, country, foundingDate = "2000-01-01" });
        return await ReadDataOrNull<ClubDto>(resp, $"Create club '{name}'");
    }

    public async Task<string?> UploadClubImageAsync(string externalImageUrl)
    {
        try
        {
            using HttpClient downloader = new();
            byte[] imageBytes = await downloader.GetByteArrayAsync(externalImageUrl);

            string fileName = Path.GetFileName(new Uri(externalImageUrl).AbsolutePath);
            if (string.IsNullOrWhiteSpace(fileName)) fileName = "logo.png";

            using MultipartFormDataContent form = new();
            ByteArrayContent fileContent = new(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg");
            form.Add(fileContent, "file", fileName);

            HttpResponseMessage resp = await _http.PostAsync("api/clubs/upload-image", form);
            if (!resp.IsSuccessStatusCode)
            {
                string body = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"  WARN: UploadClubImage failed ({(int)resp.StatusCode}): {body}");
                return null;
            }

            ApiResponse<string>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<string>>(_json);
            return api?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  WARN: Could not download/upload club image from '{externalImageUrl}': {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateClubLogoAsync(Guid clubId, string logoUrl)
    {
        HttpResponseMessage resp = await _http.PatchAsync($"api/clubs/{clubId}/logo",
            JsonContent.Create(logoUrl, mediaType: new MediaTypeHeaderValue("application/json")));
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: UpdateClubLogo failed: {body}");
        }
        return resp.IsSuccessStatusCode;
    }

    // ── Divisions ─────────────────────────────────────────

    public async Task<List<DivisionDto>> GetDivisionsAsync()
    {
        return await GetPaginatedListAsync<DivisionDto>("api/divisions?Page=1&PageSize=100");
    }

    public async Task<DivisionDto?> CreateDivisionAsync(string name, string description, int level, string sportType = "Floorball")
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/divisions", new { name, description, level, sportType });
        return await ReadDataOrNull<DivisionDto>(resp, $"Create division '{name}'");
    }

    // ── Persons ───────────────────────────────────────────

    public async Task<List<PersonDto>> SearchPersonsAsync(string name)
    {
        HttpResponseMessage resp = await _http.GetAsync($"api/persons/search?name={Uri.EscapeDataString(name)}");
        if (!resp.IsSuccessStatusCode) return [];
        ApiResponse<List<PersonDto>>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<List<PersonDto>>>(_json);
        return api?.Data ?? [];
    }

    public async Task<PersonDto?> CreatePersonAsync(string firstName, string lastName)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/persons", new { firstName, lastName, isRegistered = true });
        return await ReadDataOrNull<PersonDto>(resp, $"Create person '{firstName} {lastName}'");
    }

    // ── Floorball Players ──────────────────────────────────

    public async Task<List<FloorballPlayerDto>> GetPlayersAsync()
    {
        return await GetPaginatedListAsync<FloorballPlayerDto>("api/floorballplayer?Page=1&PageSize=100&IsActive=");
    }

    public async Task<FloorballPlayerDto?> CreatePlayerAsync(Guid personId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballplayer", new { personId });
        return await ReadDataOrNull<FloorballPlayerDto>(resp, $"Create player for person {personId}");
    }

    // ── Floorball Teams ────────────────────────────────────

    public async Task<List<FloorballTeamDto>> GetTeamsAsync()
    {
        return await GetPaginatedListAsync<FloorballTeamDto>("api/floorballteam?Page=1&PageSize=100");
    }

    public async Task<FloorballTeamDto?> CreateTeamAsync(string name, Guid clubId, Guid? divisionId, string homeArena = "MAHL Arena", string primaryColor = "Blue")
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballteam", new
        {
            name,
            clubId,
            divisionId,
            homeArena,
            primaryJerseyColor = primaryColor,
            teamCategory = "Adult",
            shortName = name.Length <= 4 ? name.ToUpperInvariant() : name[..3].ToUpperInvariant()
        });
        return await ReadDataOrNull<FloorballTeamDto>(resp, $"Create team '{name}'");
    }

    public async Task<FloorballTeamDto?> GetTeamByIdAsync(Guid teamId)
    {
        HttpResponseMessage resp = await _http.GetAsync($"api/floorballteam/{teamId}");
        return await ReadDataOrNull<FloorballTeamDto>(resp, $"Get team {teamId}");
    }

    public async Task<bool> UpdateTeamLogoAsync(Guid teamId, string logoUrl)
    {
        HttpResponseMessage resp = await _http.PatchAsync($"api/floorballteam/{teamId}/logo",
            JsonContent.Create(logoUrl, mediaType: new MediaTypeHeaderValue("application/json")));
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: UpdateTeamLogo failed: {body}");
        }
        return resp.IsSuccessStatusCode;
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
            if (body.Contains("already", StringComparison.OrdinalIgnoreCase)) return true;
            Console.WriteLine($"  WARN: Add player to team failed: {body}");
            return false;
        }
        return true;
    }

    // ── Floorball Referees ──────────────────────────────────

    public async Task<List<FloorballRefereeDto>> GetRefereesAsync()
    {
        return await GetPaginatedListAsync<FloorballRefereeDto>("api/floorballreferee?page=1&pageSize=100");
    }

    public async Task<FloorballRefereeDto?> CreateRefereeAsync(Guid personId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballreferee", new
        {
            personId,
            licenseIssueDate = "2020-01-01",
            licenseExpiryDate = "2030-12-31"
        });
        return await ReadDataOrNull<FloorballRefereeDto>(resp, $"Create referee for person {personId}");
    }

    // ── Floorball Seasons ──────────────────────────────────

    public async Task<List<FloorballSeasonDto>> GetSeasonsAsync()
    {
        return await GetPaginatedListAsync<FloorballSeasonDto>("api/floorballseason?Page=1&PageSize=100");
    }

    public async Task<FloorballSeasonDto?> CreateSeasonAsync(string name, Guid divisionId, DateTime startDate, DateTime endDate)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballseason", new
        {
            name,
            divisionIds = new[] { divisionId },
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd"),
            numberOfPeriods = 2,
            periodDurationMinutes = 15,
            allowOvertime = false,
            allowShootout = false
        });
        return await ReadDataOrNull<FloorballSeasonDto>(resp, $"Create season '{name}'");
    }

    public async Task<bool> ActivateSeasonAsync(Guid seasonId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorballseason/{seasonId}/activate", null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: ActivateSeason failed: {body}");
        }
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> AddTeamToSeasonAsync(Guid seasonId, Guid teamId)
    {
        HttpResponseMessage resp = await _http.PostAsync($"api/floorballseason/{seasonId}/teams/{teamId}", null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            if (body.Contains("already", StringComparison.OrdinalIgnoreCase)) return true;
            Console.WriteLine($"  WARN: AddTeamToSeason failed: {body}");
            return false;
        }
        return true;
    }

    public async Task<bool> AddTeamToSeasonDivisionAsync(Guid seasonId, Guid divisionId, Guid teamId)
    {
        HttpResponseMessage resp = await _http.PostAsync($"api/floorballseason/{seasonId}/divisions/{divisionId}/teams/{teamId}", null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            if (body.Contains("already", StringComparison.OrdinalIgnoreCase)) return true;
            Console.WriteLine($"  WARN: AddTeamToSeasonDivision failed: {body}");
            return false;
        }
        return true;
    }

    // ── Floorball Matches ──────────────────────────────────

    public async Task<FloorballMatchDto?> CreateMatchAsync(Guid seasonId, Guid homeTeamId, Guid awayTeamId, DateTime scheduledDateTime, string? venue)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballmatch", new
        {
            seasonId,
            homeTeamId,
            awayTeamId,
            scheduledDateTime = scheduledDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
            venue
        });
        return await ReadDataOrNull<FloorballMatchDto>(resp, "Create match");
    }

    public async Task<bool> AddOfficialToMatchAsync(Guid matchId, Guid refereeId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync($"api/floorballmatch/{matchId}/officials", new { refereeId });
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> SetGoalieAsync(Guid matchId, Guid teamId, Guid goaliePlayerId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorballmatch/{matchId}/team/{teamId}/goalie/{goaliePlayerId}", null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: SetGoalie failed: {body}");
        }
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> StartMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorballmatch/start-match/{matchId}", null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: StartMatch failed: {body}");
        }
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> StartPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await _http.PostAsync($"api/floorballmatch/{matchId}/period/{periodNumber}/start", null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> EndPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await _http.PostAsync($"api/floorballmatch/{matchId}/period/{periodNumber}/end", null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> RecordGoalAsync(Guid matchId, Guid scoringTeamId, Guid scoringPlayerId, Guid? assistingPlayerId, int periodNumber, int timeInSeconds)
    {
        object request = new
        {
            matchId,
            scoringTeamId,
            scoringPlayerId,
            assistingPlayerId,
            periodNumber,
            timeInSeconds,
        };

        return await PostWithRetryAsync("api/floorballmatch/record-goal", request, "RecordGoal");
    }

    public async Task<bool> RecordPenaltyAsync(Guid matchId, Guid teamId, Guid playerId, int durationMinutes, int periodNumber, int timeInSeconds, string penaltyType = "Minor")
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

        return await PostWithRetryAsync("api/floorballmatch/record-penalty", request, "RecordPenalty");
    }

    public async Task<bool> RecordSaveAsync(Guid matchId, Guid teamId, Guid goaliePlayerId, int periodNumber, int timeInSeconds)
    {
        object request = new
        {
            matchId,
            teamId,
            playerId = goaliePlayerId,
            periodNumber,
            timeInSeconds,
            wasInOvertime = false,
            wasInShootout = false,
        };

        return await PostWithRetryAsync("api/floorballmatch/record-save", request, "RecordSave");
    }

    private async Task<bool> PostWithRetryAsync(string url, object payload, string operationName, int maxRetries = 3)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            HttpResponseMessage resp = await _http.PostAsJsonAsync(url, payload);
            if (resp.IsSuccessStatusCode)
                return true;

            if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < maxRetries)
            {
                await Task.Delay(100);
                continue;
            }

            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"    WARN: {operationName} failed: {body}");
            return false;
        }
        return false;
    }

    public async Task<bool> CompleteMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorballmatch/complete-match/{matchId}", null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: CompleteMatch failed: {body}");
        }
        return resp.IsSuccessStatusCode;
    }

    // ── Helpers ─────────────────────────────────────────────

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
            Console.WriteLine($"  WARN: {operation} failed ({(int)resp.StatusCode}): {body}");
            return null;
        }
        ApiResponse<T>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(_json);
        return api?.Data;
    }

    private static async Task EnsureSuccess(HttpResponseMessage resp, string operation)
    {
        if (resp.IsSuccessStatusCode) return;
        string body = await resp.Content.ReadAsStringAsync();
        throw new HttpRequestException($"{operation} failed ({(int)resp.StatusCode}): {body}");
    }

    public void Dispose() => _http.Dispose();

    private class LoginDevResponse
    {
        public string? DevCode { get; set; }
    }

    private class AuthTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
