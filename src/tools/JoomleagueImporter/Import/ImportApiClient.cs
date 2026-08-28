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

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private string? _accessToken;
    private string? _refreshToken;
    private DateTime _accessExpiresAtUtc = DateTime.MinValue;

    public ImportApiClient(string baseUrl)
    {
        TokenRefreshHandler handler = new(this)
        {
            InnerHandler = new HttpClientHandler(),
        };
        Http = new HttpClient(handler)
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

    /// <summary>
    /// Local Development login (auto-fill code). Does not work against Azure.
    /// </summary>
    public async Task AuthenticateAsync(string? email = null)
    {
        string loginEmail = string.IsNullOrWhiteSpace(email) ? DefaultAuthEmail : email.Trim();
        Console.WriteLine($"Authenticating as {loginEmail} (Development auto-fill)...");

        HttpResponseMessage loginResp = await Http.PostAsJsonAsync("api/auth/login", new { email = loginEmail });
        await EnsureSuccess(loginResp, "Login");

        ApiResponse<LoginAutoFillResponse>? loginApi =
            await loginResp.Content.ReadFromJsonAsync<ApiResponse<LoginAutoFillResponse>>(Json);
        if (string.IsNullOrEmpty(loginApi?.Data?.AutoFillCode))
            throw new InvalidOperationException(
                "Login response contained no auto-fill code. For a remote API pass --access-token / --refresh-token. Locally, the API must run in Development with LoginCode:AutoFillLoginCode = true.");

        HttpResponseMessage verifyResp = await Http.PostAsJsonAsync("api/auth/verify",
            new { email = loginEmail, code = loginApi.Data.AutoFillCode });
        await EnsureSuccess(verifyResp, "Verify");

        ApiResponse<AuthTokenResponse>? verifyApi =
            await verifyResp.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>(Json);
        if (verifyApi?.Data?.AccessToken == null)
            throw new InvalidOperationException("Failed to get access token.");

        ApplyTokens(verifyApi.Data.AccessToken, verifyApi.Data.RefreshToken, verifyApi.Data.ExpiresAt);
        Console.WriteLine("Authenticated successfully.\n");
    }

    /// <summary>
    /// Use a browser/session token for a remote API. A refresh token is refreshed immediately
    /// and then kept alive for long imports.
    /// </summary>
    public async Task AuthenticateWithTokensAsync(string? accessToken, string? refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            _refreshToken = refreshToken.Trim();
            _accessExpiresAtUtc = DateTime.MinValue;
            try
            {
                await RefreshTokensAsync(force: true);
                Console.WriteLine("Authenticated with provided token (refresh applied).\n");
                return;
            }
            catch (Exception ex) when (!string.IsNullOrWhiteSpace(accessToken))
            {
                Console.WriteLine($"Refresh failed ({ex.Message}); using provided access token.");
                ApplyTokens(accessToken.Trim(), _refreshToken, DateTime.UtcNow.AddMinutes(10));
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("Provide --access-token and/or --refresh-token, or use local Development login.");

        ApplyTokens(accessToken.Trim(), refreshToken: null, DateTime.UtcNow.AddMinutes(12));
        Console.WriteLine("Authenticated with provided access token (no refresh token — session ends when the JWT expires).\n");
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

    /// <summary>
    /// Search by full name, then create. If create races another import of the same name,
    /// search again before giving up.
    /// </summary>
    public async Task<(PersonDto? Person, bool Created)> FindOrCreatePersonAsync(
        string firstName,
        string lastName,
        DateTime? birthDate)
    {
        PersonDto? existing = FindPersonByName(await SearchPersonsAsync($"{firstName} {lastName}".Trim()), firstName, lastName);
        if (existing != null)
            return (existing, false);

        PersonDto? created = await CreatePersonAsync(firstName, lastName, birthDate);
        if (created != null)
            return (created, true);

        PersonDto? retry = FindPersonByName(await SearchPersonsAsync($"{firstName} {lastName}".Trim()), firstName, lastName);
        return (retry, false);
    }

    private static PersonDto? FindPersonByName(List<PersonDto> results, string firstName, string lastName) =>
        results.FirstOrDefault(p =>
            string.Equals(p.FirstName, firstName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.LastName, lastName, StringComparison.OrdinalIgnoreCase));

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

    protected async Task<T?> PostDataOrNullAsync<T>(
        string url,
        object payload,
        string operation,
        int maxRetries = 0) where T : class
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            HttpResponseMessage resp = await Http.PostAsJsonAsync(url, payload);
            if (resp.IsSuccessStatusCode)
            {
                ApiResponse<T>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(Json);
                return api?.Data;
            }

            string body = await resp.Content.ReadAsStringAsync();
            bool retryable = attempt < maxRetries && IsRetryableUniqueConflict(resp.StatusCode, body);
            if (retryable)
            {
                await Task.Delay(200 * (attempt + 1));
                continue;
            }

            Console.WriteLine($"  WARN: {operation} failed ({(int)resp.StatusCode}): {Truncate(body)}");
            return null;
        }

        return null;
    }

    private static bool IsRetryableUniqueConflict(System.Net.HttpStatusCode statusCode, string body)
    {
        if (statusCode == System.Net.HttpStatusCode.Conflict)
            return true;

        return body.Contains("concurrent", StringComparison.OrdinalIgnoreCase)
            || body.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || body.Contains("23505", StringComparison.Ordinal);
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

    private void ApplyTokens(string accessToken, string? refreshToken, DateTime expiresAtUtc)
    {
        _accessToken = accessToken;
        if (!string.IsNullOrWhiteSpace(refreshToken))
            _refreshToken = refreshToken;
        _accessExpiresAtUtc = expiresAtUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc)
            : expiresAtUtc.ToUniversalTime();
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    internal async Task EnsureFreshTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_refreshToken))
            return;
        if (_accessExpiresAtUtc > DateTime.UtcNow.AddMinutes(3))
            return;
        await RefreshTokensAsync(cancellationToken);
    }

    internal async Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_refreshToken))
            return false;
        try
        {
            await RefreshTokensAsync(cancellationToken, force: true);
            return true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"  WARN: token refresh failed: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"  WARN: token refresh failed: {ex.Message}");
            return false;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"  WARN: token refresh failed: {ex.Message}");
            return false;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  WARN: token refresh failed: {ex.Message}");
            return false;
        }
    }

    private async Task RefreshTokensAsync(CancellationToken cancellationToken = default, bool force = false)
    {
        if (string.IsNullOrWhiteSpace(_refreshToken))
            throw new InvalidOperationException("No refresh token is available.");

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (!force && _accessExpiresAtUtc > DateTime.UtcNow.AddMinutes(3) && !string.IsNullOrEmpty(_accessToken))
                return;

            HttpResponseMessage resp = await Http.PostAsJsonAsync(
                "api/auth/refresh",
                new { refreshToken = _refreshToken },
                cancellationToken);
            await EnsureSuccess(resp, "Refresh token");

            ApiResponse<AuthTokenResponse>? api =
                await resp.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>(Json, cancellationToken);
            if (api?.Data?.AccessToken == null)
                throw new InvalidOperationException("Refresh returned no access token.");

            ApplyTokens(api.Data.AccessToken, api.Data.RefreshToken, api.Data.ExpiresAt);
            Console.WriteLine($"Token refreshed, expires {api.Data.ExpiresAt:u}.");
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void Dispose()
    {
        Http.Dispose();
        _refreshLock.Dispose();
    }

    private sealed class TokenRefreshHandler : DelegatingHandler
    {
        private readonly ImportApiClient _owner;

        public TokenRefreshHandler(ImportApiClient owner)
        {
            _owner = owner;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            bool isRefresh = IsAuthRefresh(request);
            if (!isRefresh)
                await _owner.EnsureFreshTokenAsync(cancellationToken);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized || isRefresh)
                return response;

            if (!await _owner.TryRefreshAsync(cancellationToken))
                return response;

            response.Dispose();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _owner._accessToken);
            return await base.SendAsync(request, cancellationToken);
        }

        private static bool IsAuthRefresh(HttpRequestMessage request)
        {
            string? path = request.RequestUri?.AbsolutePath;
            return path != null && path.Contains("/auth/refresh", StringComparison.OrdinalIgnoreCase);
        }
    }

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
