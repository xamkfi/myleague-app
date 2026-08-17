using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TournamentExporter;

internal sealed class SourceApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public SourceApiClient(string baseUrl)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(NormalizeBaseUrl(baseUrl)),
            Timeout = TimeSpan.FromMinutes(2),
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

    public async Task<SourceTournament> GetTournamentAsync(Guid tournamentId)
    {
        SourceTournament? tournament = await ReadDataAsync<SourceTournament>(
            $"api/FloorballTournament/{tournamentId}",
            $"Get tournament {tournamentId}");
        if (tournament is null)
            throw new InvalidOperationException($"Tournament {tournamentId} was not found.");
        return tournament;
    }

    public async Task<List<SourceMatch>> GetMatchesAsync(Guid tournamentId)
    {
        List<SourceMatch>? matches = await ReadDataAsync<List<SourceMatch>>(
            $"api/floorball-matches/by-competitionId/{tournamentId}",
            $"Get matches for {tournamentId}");
        return matches ?? [];
    }

    public async Task<SourceMatch?> GetMatchByIdAsync(Guid matchId)
    {
        return await ReadDataAsync<SourceMatch>(
            $"api/floorball-matches/by-id/{matchId}",
            $"Get match {matchId}");
    }

    public async Task HydrateMatchEventsAsync(List<SourceMatch> matches)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            SourceMatch? detailed = await GetMatchByIdAsync(matches[i].Id);
            if (detailed is not null)
                matches[i] = detailed;

            if ((i + 1) % 5 == 0 || i == matches.Count - 1)
                Console.WriteLine($"  Hydrated {i + 1}/{matches.Count} matches with events");
        }
    }

    public async Task<SourceTeam?> GetTeamAsync(Guid teamId)
    {
        return await ReadDataAsync<SourceTeam>($"api/FloorballTeam/{teamId}", $"Get team {teamId}");
    }

    private async Task<T?> ReadDataAsync<T>(string url, string operation) where T : class
    {
        HttpResponseMessage response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"  WARN: {operation} failed ({(int)response.StatusCode}): {body}");
            return null;
        }

        ApiResponse<T>? api = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(_json);
        if (api is null || !api.Success)
        {
            Console.WriteLine($"  WARN: {operation} returned an unsuccessful payload: {api?.Message}");
            return null;
        }

        return api.Data;
    }

    internal static string NormalizeBaseUrl(string url)
    {
        string trimmed = url.Trim();
        if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = "https://" + trimmed;
        }

        trimmed = trimmed.TrimEnd('/');
        if (trimmed.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[..^4];

        return trimmed + "/";
    }

    public void Dispose() => _http.Dispose();
}
