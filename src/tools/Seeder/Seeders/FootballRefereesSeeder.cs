using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Football.Referees.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace Seeder;

public static class FootballRefereesSeeder
{
    public static async Task<List<FootballRefereeDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<Guid> personIds)
    {
        List<FootballRefereeDto> created = new List<FootballRefereeDto>();

        if (personIds.Count == 0)
        {
            Console.WriteLine("Football referees seed: no person IDs provided, nothing to seed.");
            return created;
        }

        Console.WriteLine($"Football referees seed: processing {personIds.Count} person ID(s).");

        foreach (Guid personId in personIds)
        {
            FootballRefereeDto? existing = await FindRefereeByPersonIdAsync(http, jsonOptions, personId);
            if (existing != null)
            {
                created.Add(existing);
                Console.WriteLine("Football referee exists for personId, skipping: " + personId + " (refereeId: " + existing.Id + ")");
                continue;
            }

            CreateFootballRefereeRequest request = new CreateFootballRefereeRequest
            {
                PersonId = personId,
                LicenseIssueDate = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
                LicenseExpiryDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd")
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/FootballReferee", request);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest &&
                    body.Contains("already a referee", StringComparison.OrdinalIgnoreCase))
                {
                    FootballRefereeDto? existingRef = await FindRefereeByPersonIdAsync(http, jsonOptions, personId);
                    if (existingRef != null)
                    {
                        created.Add(existingRef);
                        Console.WriteLine("Person already is a football referee, using existing: " + personId + " (refereeId: " + existingRef.Id + ")");
                    }
                    else
                    {
                        Console.Error.WriteLine(
                            $"WARNING: person {personId} is already a football referee in the database, but the GET /api/FootballReferee listing did not return them.");
                    }
                    continue;
                }
                throw new HttpRequestException("Create Football Referee failed with " + (int)response.StatusCode + " " + response.StatusCode + ": " + body);
            }

            ApiResponse<FootballRefereeDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FootballRefereeDto>>(jsonOptions);
            if (api == null || !api.Success || api.Data == null)
            {
                throw new InvalidOperationException("Create football referee failed: " + (api != null ? api.Message : "null response"));
            }

            created.Add(api.Data);
            Console.WriteLine("Created football referee for personId " + personId + " (refereeId: " + api.Data.Id + ")");
        }

        Console.WriteLine($"Football referees seed: produced {created.Count} referee record(s) (created or pre-existing).");
        return created;
    }

    public static async Task<List<FootballRefereeDto>> FetchAllRefereesFromApiAsync(HttpClient http, JsonSerializerOptions jsonOptions)
    {
        List<FootballRefereeDto> all = new List<FootballRefereeDto>();
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage resp = await http.GetAsync($"api/FootballReferee?page={page}&pageSize={pageSize}");
            if (!resp.IsSuccessStatusCode)
            {
                string body = await resp.Content.ReadAsStringAsync();
                Console.Error.WriteLine(
                    $"WARNING: FetchAllRefereesFromApiAsync got {(int)resp.StatusCode} {resp.StatusCode} from GET /api/FootballReferee?page={page}&pageSize={pageSize}. " +
                    $"Body: {(body.Length > 500 ? body.Substring(0, 500) + "..." : body)}");
                break;
            }

            PaginatedApiResponse<FootballRefereeDto>? api = await resp.Content.ReadFromJsonAsync<PaginatedApiResponse<FootballRefereeDto>>(jsonOptions);
            if (api?.Data == null || !api.Data.Any())
            {
                break;
            }

            all.AddRange(api.Data);
            if (api.Data.Count() < pageSize)
            {
                break;
            }

            page++;
            if (page > 50)
            {
                break;
            }
        }

        return all;
    }

    public static Dictionary<string, Guid> BuildEmailToRefereeIdMap(
        List<FootballRefereeDto> referees,
        Dictionary<string, Guid> seedEmailToPersonId)
    {
        Dictionary<string, Guid> map = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, Guid> kvp in seedEmailToPersonId)
        {
            FootballRefereeDto? referee = referees.FirstOrDefault(r => r.PersonId == kvp.Value);
            if (referee != null)
            {
                map[kvp.Key] = referee.Id;
            }
        }

        return map;
    }

    private static async Task<FootballRefereeDto?> FindRefereeByPersonIdAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid personId)
    {
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage listResp = await http.GetAsync($"api/FootballReferee?page={page}&pageSize={pageSize}");
            if (!listResp.IsSuccessStatusCode)
            {
                string body = await listResp.Content.ReadAsStringAsync();
                Console.Error.WriteLine(
                    $"WARNING: GET /api/FootballReferee?page={page}&pageSize={pageSize} returned {(int)listResp.StatusCode} {listResp.StatusCode}. " +
                    $"Body: {Truncate(body, 500)}");
                return null;
            }

            PaginatedApiResponse<FootballRefereeDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FootballRefereeDto>>(jsonOptions);
            if (listApi == null || listApi.Data == null)
            {
                return null;
            }

            FootballRefereeDto? found = listApi.Data.FirstOrDefault(r => r.PersonId == personId);
            if (found != null)
            {
                return found;
            }

            if (listApi.Data.Count() < pageSize)
            {
                return null;
            }

            page++;
            if (page > 50)
            {
                return null;
            }
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value ?? string.Empty;
        }
        return value.Substring(0, maxLength) + "...";
    }
}
