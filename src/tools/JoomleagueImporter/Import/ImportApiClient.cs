using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using JoomleagueImporter.Models;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace JoomleagueImporter.Import;

/// <summary>
/// Shared HTTP client for auth and common resources (clubs, divisions, persons).
/// Sport-specific routes live on <see cref="FloorballApiClient"/>,
/// <see cref="FootballApiClient"/>, and <see cref="HockeyApiClient"/>.
/// </summary>
public class ImportApiClient : IDisposable
{
    private const string DefaultAuthEmail = "test@myleague.local";

    protected HttpClient Http { get; }
    protected JsonSerializerOptions Json { get; }

    public ImportApiClient(string baseUrl)
    {
        Http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(2),
        };
        Http.DefaultRequestHeaders.Accept.Clear();
        Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Json = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        Json.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task AuthenticateAsync(string? email = null)
    {
        string loginEmail = string.IsNullOrWhiteSpace(email) ? DefaultAuthEmail : email.Trim();
        Console.WriteLine($"Authenticating as {loginEmail}...");

        HttpResponseMessage loginResp = await Http.PostAsJsonAsync("api/auth/login", new { email = loginEmail });
        await EnsureSuccess(loginResp, "Login");

        ApiResponse<LoginAutoFillResponse>? loginApi =
            await loginResp.Content.ReadFromJsonAsync<ApiResponse<LoginAutoFillResponse>>(Json);
        if (string.IsNullOrEmpty(loginApi?.Data?.AutoFillCode))
            throw new InvalidOperationException(
                "Login response contained no auto-fill code. Is the API running in Development mode with LoginCode:AutoFillLoginCode = true?");

        HttpResponseMessage verifyResp = await Http.PostAsJsonAsync("api/auth/verify",
            new { email = loginEmail, code = loginApi.Data.AutoFillCode });
        await EnsureSuccess(verifyResp, "Verify");

        ApiResponse<AuthTokenResponse>? verifyApi =
            await verifyResp.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>(Json);
        if (verifyApi?.Data?.AccessToken == null)
            throw new InvalidOperationException("Failed to get access token.");

        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", verifyApi.Data.AccessToken);
        Console.WriteLine("Authenticated successfully.\n");
    }

    public async Task<List<ClubDto>> GetClubsAsync() =>
        await GetPaginatedListAsync<ClubDto>("api/clubs?Page=1&PageSize=50");

    public async Task<ClubDto?> CreateClubAsync(string name, string city, string country = "Finland")
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/clubs",
            new { name, city, country, foundingDate = "2000-01-01" });
        return await ReadDataOrNull<ClubDto>(resp, $"Create club '{name}'");
    }

    public async Task<List<DivisionDto>> GetDivisionsAsync() =>
        await GetPaginatedListAsync<DivisionDto>("api/divisions?Page=1&PageSize=50");

    public async Task<DivisionDto?> CreateDivisionAsync(string name, string description, int level, string sportType = "Floorball")
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/divisions", new { name, description, level, sportType });
        return await ReadDataOrNull<DivisionDto>(resp, $"Create division '{name}'");
    }

    public async Task EnsureSeasonContentBlocksAsync(string routePrefix, Guid seasonId, OldProject project)
    {
        HttpResponseMessage get = await Http.GetAsync($"{routePrefix}/{seasonId}/content-blocks");
        SeasonContentBlocksPayload? existing =
            await ReadDataOrNull<SeasonContentBlocksPayload>(get, $"Get season content blocks {seasonId}");
        if (existing?.Blocks is { Count: > 0 })
        {
            Console.WriteLine($"  Season content blocks already present ({existing.Blocks.Count}), skipping.");
            return;
        }

        List<SeasonContentBlockPutItem> items = SeasonContentFromProject.BuildItems(project);
        HttpResponseMessage put = await Http.PutAsJsonAsync(
            $"{routePrefix}/{seasonId}/content-blocks",
            new { items });
        if (await OkOrWarn(put, "Replace season content blocks"))
            Console.WriteLine($"  Set {items.Count} content block(s) for '{project.Name}'");
    }

    public async Task<List<PersonDto>> SearchPersonsAsync(string name)
    {
        HttpResponseMessage resp = await Http.GetAsync(
            $"api/persons/search?name={Uri.EscapeDataString(name)}&page=1&PageSize=50");
        if (!resp.IsSuccessStatusCode) return [];
        PaginatedApiResponse<PersonDto>? api =
            await resp.Content.ReadFromJsonAsync<PaginatedApiResponse<PersonDto>>(Json);
        return api?.Data?.ToList() ?? [];
    }

    public async Task<PersonDto?> CreatePersonAsync(string firstName, string lastName, DateTime? birthDate = null)
    {
        HttpResponseMessage resp = await Http.PostAsJsonAsync("api/persons", new
        {
            firstName,
            lastName,
            birthDate = birthDate?.ToString("yyyy-MM-dd"),
            isRegistered = false,
        });
        return await ReadDataOrNull<PersonDto>(resp, $"Create person '{firstName} {lastName}'");
    }

    protected async Task<bool> AddPlayerToTeamByQueryAsync(
        string url,
        int? jerseyNumber,
        string warnLabel)
    {
        string requestUrl = jerseyNumber.HasValue ? $"{url}&jerseyNumber={jerseyNumber.Value}" : url;

        HttpResponseMessage resp = await Http.PostAsync(requestUrl, null);
        if (resp.IsSuccessStatusCode)
            return true;

        string body = await resp.Content.ReadAsStringAsync();
        if (body.Contains("is already in the roster", StringComparison.OrdinalIgnoreCase))
            return true;

        if (jerseyNumber.HasValue &&
            body.Contains("Jersey number", StringComparison.OrdinalIgnoreCase))
        {
            return await AddPlayerToTeamByQueryAsync(url, null, warnLabel);
        }

        Console.WriteLine($"  WARN: {warnLabel} failed: {Truncate(body)}");
        return false;
    }

    protected async Task<bool> PostWithRetryAsync(string url, object payload, string operationName, int maxRetries = 5)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            HttpResponseMessage resp = await Http.PostAsJsonAsync(url, payload);
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

    protected async Task<List<T>> GetPaginatedListAsync<T>(string url)
    {
        List<T> all = [];
        string currentUrl = url;

        while (true)
        {
            HttpResponseMessage resp = await Http.GetAsync(currentUrl);
            if (!resp.IsSuccessStatusCode) break;

            PaginatedApiResponse<T>? api = await resp.Content.ReadFromJsonAsync<PaginatedApiResponse<T>>(Json);
            if (api?.Data != null)
                all.AddRange(api.Data);

            if (api?.Pagination.HasNextPage != true) break;

            int nextPage = api.Pagination.CurrentPage + 1;
            currentUrl = Regex.Replace(url, @"Page=\d+", $"Page={nextPage}", RegexOptions.IgnoreCase);
        }

        return all;
    }

    protected async Task<List<T>> GetUnpaginatedListAsync<T>(string url)
    {
        HttpResponseMessage resp = await Http.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
            return [];

        ApiResponse<List<T>>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<List<T>>>(Json);
        return api?.Data ?? [];
    }

    protected async Task<T?> ReadDataOrNull<T>(HttpResponseMessage resp, string operation) where T : class
    {
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: {operation} failed ({(int)resp.StatusCode}): {Truncate(body)}");
            return null;
        }
        ApiResponse<T>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(Json);
        return api?.Data;
    }

    protected static async Task<bool> OkOrWarn(HttpResponseMessage resp, string operation)
    {
        if (resp.IsSuccessStatusCode) return true;
        string body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"  WARN: {operation} failed: {Truncate(body)}");
        return false;
    }

    protected static async Task<bool> OkOrAlready(HttpResponseMessage resp, string operation)
    {
        if (resp.IsSuccessStatusCode) return true;
        string body = await resp.Content.ReadAsStringAsync();
        if (body.Contains("already", StringComparison.OrdinalIgnoreCase)
            || body.Contains("in status Active", StringComparison.OrdinalIgnoreCase)
            || body.Contains("status is Active", StringComparison.OrdinalIgnoreCase))
            return true;
        Console.WriteLine($"  WARN: {operation} failed: {Truncate(body)}");
        return false;
    }

    protected static string Truncate(string s) => s.Length <= 300 ? s : s[..300] + "...";

    protected static string UtcDate(string yyyyMmDd) => $"{yyyyMmDd}T00:00:00Z";

    protected static string UtcDateTime(DateTime value)
    {
        DateTime utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
        return utc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
    }

    private static async Task EnsureSuccess(HttpResponseMessage resp, string operation)
    {
        if (resp.IsSuccessStatusCode) return;
        string body = await resp.Content.ReadAsStringAsync();
        throw new HttpRequestException($"{operation} failed ({(int)resp.StatusCode}): {body}");
    }

    public void Dispose() => Http.Dispose();

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
