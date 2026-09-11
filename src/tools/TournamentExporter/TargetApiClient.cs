using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TournamentExporter;

internal sealed class TargetApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public TargetApiClient(string baseUrl)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(SourceApiClient.NormalizeBaseUrl(baseUrl)),
            Timeout = TimeSpan.FromMinutes(10),
        };
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _json = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        _json.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task AuthenticateAsync(string email)
    {
        Console.WriteLine($"Authenticating as {email}...");
        HttpResponseMessage loginResp = await _http.PostAsJsonAsync("api/auth/login", new { email });
        await EnsureSuccess(loginResp, "Login");

        ApiResponse<LoginDevResponse>? loginApi = await loginResp.Content.ReadFromJsonAsync<ApiResponse<LoginDevResponse>>(_json);
        string? code = loginApi?.Data?.DevCode ?? loginApi?.Data?.AutoFillCode;
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Login returned no dev/auto-fill code. Is the API in Development with AutoFillLoginCode?");

        HttpResponseMessage verifyResp = await _http.PostAsJsonAsync("api/auth/verify", new { email, code });
        await EnsureSuccess(verifyResp, "Verify");

        ApiResponse<AuthTokenResponse>? verifyApi = await verifyResp.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>(_json);
        if (string.IsNullOrWhiteSpace(verifyApi?.Data?.AccessToken))
            throw new InvalidOperationException("Verify returned no access token.");

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", verifyApi.Data.AccessToken);
        Console.WriteLine("Authenticated.\n");
    }

    public async Task<IdName?> FindClubAsync(string name)
    {
        HttpResponseMessage resp = await _http.GetAsync($"api/clubs/search?name={Uri.EscapeDataString(name)}");
        if (!resp.IsSuccessStatusCode)
            return null;
        ApiResponse<List<IdName>>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<List<IdName>>>(_json);
        return api?.Data?.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IdName> CreateClubAsync(ExportClub club)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/clubs", new
        {
            name = club.Name,
            city = club.City,
            country = club.Country,
            websiteUrl = club.WebsiteUrl,
            logoUrl = club.LogoUrl,
            contactEmail = club.ContactEmail,
        });
        IdName? created = await ReadRequiredAsync<IdName>(resp, $"Create club '{club.Name}'");
        return created;
    }

    public async Task<IdName?> FindTeamAsync(string name)
    {
        HttpResponseMessage resp = await _http.GetAsync($"api/floorballteam/names?nameFilter={Uri.EscapeDataString(name)}");
        if (!resp.IsSuccessStatusCode)
            return null;
        ApiResponse<List<IdName>>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<List<IdName>>>(_json);
        return api?.Data?.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IdName> CreateTeamAsync(ExportTeam team, Guid clubId, string category)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballteam", new
        {
            name = team.Name,
            clubId,
            homeArena = team.HomeArena,
            primaryJerseyColor = team.PrimaryJerseyColor,
            secondaryJerseyColor = team.SecondaryJerseyColor,
            category,
            shortName = MakeShortName(team.Name),
        });
        IdName? created = await ReadRequiredAsync<IdName>(resp, $"Create team '{team.Name}'");
        return created;
    }

    public async Task<SourceTeam?> GetTeamAsync(Guid teamId)
    {
        return await ReadOptionalAsync<SourceTeam>(await _http.GetAsync($"api/FloorballTeam/{teamId}"));
    }

    public async Task<IdName?> FindPersonAsync(string firstName, string lastName)
    {
        string term = $"{firstName} {lastName}".Trim();
        HttpResponseMessage resp = await _http.GetAsync($"api/persons/search?name={Uri.EscapeDataString(term)}");
        if (!resp.IsSuccessStatusCode)
            return null;
        ApiResponse<List<PersonName>>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<List<PersonName>>>(_json);
        PersonName? match = api?.Data?.FirstOrDefault(p =>
            string.Equals(p.FirstName, firstName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(p.LastName, lastName, StringComparison.OrdinalIgnoreCase));
        return match is null ? null : new IdName { Id = match.Id, Name = match.FullName ?? $"{match.FirstName} {match.LastName}" };
    }

    public async Task<IdName> CreatePersonAsync(string firstName, string lastName)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/persons", new
        {
            firstName,
            lastName,
            isRegistered = false,
        });
        IdName? created = await ReadRequiredAsync<IdName>(resp, $"Create person '{firstName} {lastName}'");
        return created;
    }

    public async Task<Guid?> FindPlayerByPersonAsync(Guid personId, string personName)
    {
        HttpResponseMessage resp = await _http.GetAsync($"api/floorballplayer?searchTerm={Uri.EscapeDataString(personName)}&pageSize=50&IsActive=");
        if (!resp.IsSuccessStatusCode)
            return null;
        PaginatedApiResponse<PlayerRow>? api = await resp.Content.ReadFromJsonAsync<PaginatedApiResponse<PlayerRow>>(_json);
        return api?.Data?.FirstOrDefault(p => p.PersonId == personId)?.Id;
    }

    public async Task<Guid> CreatePlayerAsync(Guid personId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballplayer", new { personId });
        IdName? created = await ReadRequiredAsync<IdName>(resp, $"Create player for {personId}");
        return created.Id;
    }

    public async Task<bool> AddPlayerToTeamAsync(Guid teamId, Guid playerId, string position, int? jerseyNumber)
    {
        string url = $"api/floorballteam/{teamId}/players/{playerId}?position={Uri.EscapeDataString(position)}";
        if (jerseyNumber.HasValue)
            url += $"&jerseyNumber={jerseyNumber.Value}";

        HttpResponseMessage resp = await _http.PostAsync(url, null);
        if (resp.IsSuccessStatusCode)
            return true;

        string body = await resp.Content.ReadAsStringAsync();
        if (body.Contains("already", StringComparison.OrdinalIgnoreCase)
            || body.Contains("jersey", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Console.WriteLine($"    WARN: add player failed ({(int)resp.StatusCode}): {TrimBody(body)}");
        return false;
    }

    public async Task<IdName?> FindTournamentAsync(string name)
    {
        HttpResponseMessage resp = await _http.GetAsync("api/floorballtournament");
        if (!resp.IsSuccessStatusCode)
            return null;
        ApiResponse<List<IdName>>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<List<IdName>>>(_json);
        return api?.Data?.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<TournamentDetail> CreateTournamentWithScheduleAsync(ExportTournament tournament, List<ExportPlayoffSlot>? schedule)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorballtournament", new
        {
            name = tournament.Name,
            startDate = tournament.StartDate,
            endDate = tournament.EndDate,
            venue = tournament.Venue,
            contentHtml = tournament.ContentHtml,
            groupStageNumberOfPeriods = tournament.GroupStageNumberOfPeriods,
            groupStagePeriodDurationMinutes = tournament.GroupStagePeriodDurationMinutes,
            groupStageAllowOvertime = tournament.GroupStageAllowOvertime,
            groupStageOvertimeDurationMinutes = tournament.GroupStageOvertimeDurationMinutes,
            groupStageAllowShootout = tournament.GroupStageAllowShootout,
            playoffNumberOfPeriods = tournament.PlayoffNumberOfPeriods,
            playoffPeriodDurationMinutes = tournament.PlayoffPeriodDurationMinutes,
            playoffAllowOvertime = tournament.PlayoffAllowOvertime,
            playoffOvertimeDurationMinutes = tournament.PlayoffOvertimeDurationMinutes,
            playoffAllowShootout = tournament.PlayoffAllowShootout,
            teamsAdvancingPerGroup = tournament.TeamsAdvancingPerGroup,
            hasPlayoffStage = tournament.HasPlayoffStage,
            hasThirdPlaceMatch = tournament.HasThirdPlaceMatch,
            playoffSchedule = schedule?.Select(s => new
            {
                round = s.Round,
                order = s.Order,
                scheduledDateTime = s.ScheduledDateTime,
                venue = s.Venue,
            }),
            teamCategory = tournament.TeamCategory,
        });
        return await ReadRequiredAsync<TournamentDetail>(resp, $"Create tournament '{tournament.Name}'");
    }

    public async Task<TournamentDetail> AddGroupAsync(Guid tournamentId, string groupName)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync($"api/floorballtournament/{tournamentId}/groups", new { groupName });
        return await ReadRequiredAsync<TournamentDetail>(resp, $"Add group '{groupName}'");
    }

    public async Task<TournamentDetail> AddTeamToGroupAsync(Guid tournamentId, Guid groupId, Guid teamId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync(
            $"api/floorballtournament/{tournamentId}/groups/{groupId}/teams",
            new { teamId });
        return await ReadRequiredAsync<TournamentDetail>(resp, "Add team to group");
    }

    public async Task DeleteTournamentAsync(Guid tournamentId)
    {
        HttpResponseMessage resp = await _http.DeleteAsync($"api/floorballtournament/{tournamentId}");
        await EnsureSuccess(resp, $"Delete tournament {tournamentId}");
    }

    public async Task<Guid> GetOrCreateImportRefereeAsync()
    {
        HttpResponseMessage listResp = await _http.GetAsync("api/floorballreferee?page=1&pageSize=10");
        if (listResp.IsSuccessStatusCode)
        {
            PaginatedApiResponse<RefereeRow>? page =
                await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<RefereeRow>>(_json);
            RefereeRow? existing = page?.Data?.FirstOrDefault();
            if (existing is not null && existing.Id != Guid.Empty)
                return existing.Id;
        }

        IdName person = await CreatePersonAsync("Import", "Referee");
        HttpResponseMessage createResp = await _http.PostAsJsonAsync("api/floorballreferee", new
        {
            personId = person.Id,
            licenseIssueDate = "2020-01-01",
            licenseExpiryDate = "2030-12-31",
        });
        RefereeRow created = await ReadRequiredAsync<RefereeRow>(createResp, "Create import referee");
        return created.Id;
    }

    public async Task<IdName> CreateMatchAsync(
        Guid tournamentId,
        Guid homeTeamId,
        Guid awayTeamId,
        string scheduledDateTime,
        string? venue,
        Guid? groupId,
        string? tournamentStage)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync("api/floorball-matches", new
        {
            competitionId = tournamentId,
            homeTeamId,
            awayTeamId,
            scheduledDateTime,
            venue,
            tournamentGroupId = groupId,
            tournamentStage = string.IsNullOrWhiteSpace(tournamentStage) ? "GroupStage" : tournamentStage,
        });
        return await ReadRequiredAsync<IdName>(resp, "Create match");
    }

    public async Task<bool> AddOfficialAsync(Guid matchId, Guid refereeId)
    {
        HttpResponseMessage resp = await _http.PostAsJsonAsync(
            $"api/floorball-matches/{matchId}/officials",
            new { refereeId });
        if (resp.IsSuccessStatusCode)
            return true;

        string body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"    WARN: add official failed ({(int)resp.StatusCode}): {TrimBody(body)}");
        return false;
    }

    public async Task<bool> SetGoalieAsync(Guid matchId, Guid teamId, Guid goalieId)
    {
        HttpResponseMessage resp = await _http.PutAsync(
            $"api/floorball-matches/{matchId}/teams/{teamId}/goalie/{goalieId}",
            null);
        if (resp.IsSuccessStatusCode)
            return true;

        string body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"    WARN: set goalie failed ({(int)resp.StatusCode}): {TrimBody(body)}");
        return false;
    }

    public async Task<bool> StartMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorball-matches/{matchId}/start", null);
        if (resp.IsSuccessStatusCode)
            return true;

        string body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"    WARN: start match failed ({(int)resp.StatusCode}): {TrimBody(body)}");
        return false;
    }

    public async Task<bool> CompleteMatchAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PutAsync($"api/floorball-matches/{matchId}/complete", null);
        if (resp.IsSuccessStatusCode)
            return true;

        string body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"    WARN: complete match failed ({(int)resp.StatusCode}): {TrimBody(body)}");
        return false;
    }

    public async Task<bool> StartPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await _http.PostAsync(
            $"api/floorball-matches/{matchId}/events/periods/{periodNumber}/start",
            null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> EndPeriodAsync(Guid matchId, int periodNumber)
    {
        HttpResponseMessage resp = await _http.PostAsync(
            $"api/floorball-matches/{matchId}/events/periods/{periodNumber}/end",
            null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> RecordOvertimeAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PostAsync($"api/floorball-matches/{matchId}/events/overtime", null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> RecordShootoutAsync(Guid matchId)
    {
        HttpResponseMessage resp = await _http.PostAsync($"api/floorball-matches/{matchId}/events/shootout", null);
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
        return await PostWithRetryAsync($"api/floorball-matches/{matchId}/events/goal", new
        {
            matchId,
            scoringTeamId,
            scoringPlayerId,
            assistingPlayerId,
            secondaryAssistingPlayerIs = secondaryAssistingPlayerId,
            periodNumber,
            timeInSeconds,
        }, "RecordGoal");
    }

    public async Task<bool> RecordPenaltyAsync(
        Guid matchId,
        Guid teamId,
        Guid playerId,
        int durationMinutes,
        int periodNumber,
        int timeInSeconds,
        string penaltyType,
        string? description)
    {
        return await PostWithRetryAsync($"api/floorball-matches/{matchId}/events/penalty", new
        {
            matchId,
            teamId,
            playerId,
            durationMinutes,
            periodNumber,
            timeInSeconds,
            penaltyType,
            description,
        }, "RecordPenalty");
    }

    public async Task<bool> RecordSaveAsync(
        Guid matchId,
        Guid teamId,
        Guid goaliePlayerId,
        int periodNumber,
        int timeInSeconds,
        bool wasInOvertime,
        bool wasInShootout)
    {
        return await PostWithRetryAsync($"api/floorball-matches/{matchId}/events/save", new
        {
            matchId,
            teamId,
            playerId = goaliePlayerId,
            periodNumber,
            timeInSeconds,
            wasInOvertime,
            wasInShootout,
        }, "RecordSave");
    }

    private async Task<bool> PostWithRetryAsync(string url, object payload, string operation, int maxRetries = 5)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            HttpResponseMessage resp = await _http.PostAsJsonAsync(url, payload);
            if (resp.IsSuccessStatusCode)
            {
                await Task.Delay(150);
                return true;
            }

            if ((int)resp.StatusCode == 429 && attempt < maxRetries)
            {
                await Task.Delay(300 * (attempt + 1));
                continue;
            }

            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"    WARN: {operation} failed ({(int)resp.StatusCode}): {TrimBody(body)}");
            return false;
        }

        return false;
    }

    private async Task<T> ReadRequiredAsync<T>(HttpResponseMessage resp, string operation) where T : class
    {
        string body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"{operation} failed ({(int)resp.StatusCode}): {TrimBody(body)}");

        ApiResponse<T>? api = JsonSerializer.Deserialize<ApiResponse<T>>(body, _json);
        if (api?.Data is null)
            throw new InvalidOperationException($"{operation} returned no data: {TrimBody(body)}");
        return api.Data;
    }

    private async Task<T?> ReadOptionalAsync<T>(HttpResponseMessage resp) where T : class
    {
        if (!resp.IsSuccessStatusCode)
            return null;
        ApiResponse<T>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(_json);
        return api?.Data;
    }

    private static async Task EnsureSuccess(HttpResponseMessage resp, string operation)
    {
        if (resp.IsSuccessStatusCode)
            return;
        string body = await resp.Content.ReadAsStringAsync();
        throw new HttpRequestException($"{operation} failed ({(int)resp.StatusCode}): {TrimBody(body)}");
    }

    private static string TrimBody(string body)
    {
        string trimmed = Regex.Replace(body, @"\s+", " ").Trim();
        return trimmed.Length > 400 ? trimmed[..400] + "…" : trimmed;
    }

    private static string MakeShortName(string name)
    {
        string letters = Regex.Replace(name, @"[^A-Za-zÄÖÅäöå0-9]", string.Empty);
        if (letters.Length == 0)
            return "TM";
        return letters.Length <= 4 ? letters.ToUpperInvariant() : letters[..3].ToUpperInvariant();
    }

    public void Dispose() => _http.Dispose();

    private sealed class LoginDevResponse
    {
        public string? DevCode { get; set; }
        public string? AutoFillCode { get; set; }
    }

    private sealed class AuthTokenResponse
    {
        public string? AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    private sealed class PersonName
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }

    private sealed class PlayerRow
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
    }

    private sealed class RefereeRow
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
    }
}

internal sealed class IdName
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

internal sealed class TournamentDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TournamentGroupDetail> Groups { get; set; } = [];
}

internal sealed class TournamentGroupDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

internal sealed class PaginatedApiResponse<T>
{
    public bool Success { get; set; }
    public List<T>? Data { get; set; }
}
