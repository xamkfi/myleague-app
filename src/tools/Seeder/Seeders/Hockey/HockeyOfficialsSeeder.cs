using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Hockey.Officials.DTOs;
using Domain.Enums.Hockey.Teams;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

/// <summary>
/// Creates hockey official profiles from Common Person ids (idempotent by PersonId).
/// </summary>
public static class HockeyOfficialsSeeder
{
    public static async Task<List<HockeyOfficialDto>> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<Guid> personIds)
    {
        List<HockeyOfficialDto> result = new List<HockeyOfficialDto>();
        if (personIds.Count == 0)
        {
            Console.WriteLine("Hockey officials seed: no person IDs provided.");
            return result;
        }

        List<HockeyOfficialDto> existing = await LoadAllOfficialsAsync(http, jsonOptions);
        Console.WriteLine("Hockey officials seed: processing " + personIds.Count + " person ID(s).");

        int badge = 10;
        foreach (Guid personId in personIds)
        {
            HockeyOfficialDto? found = existing.FirstOrDefault(o => o.PersonId == personId);
            if (found != null)
            {
                result.Add(found);
                Console.WriteLine("Hockey official exists for personId " + personId + " (" + found.Id + "), skipping");
                continue;
            }

            CreateHockeyOfficialRequest request = new CreateHockeyOfficialRequest
            {
                PersonId = personId,
                OfficialRole = HockeyOfficialRole.Referee,
                OfficialNumber = badge.ToString(),
                LicenseIssueDate = DateTime.UtcNow.AddYears(-1),
                LicenseExpiryDate = DateTime.UtcNow.AddYears(2)
            };
            badge++;

            HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyOfficial", request);
            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                if (body.Contains("already", StringComparison.OrdinalIgnoreCase))
                {
                    existing = await LoadAllOfficialsAsync(http, jsonOptions);
                    found = existing.FirstOrDefault(o => o.PersonId == personId);
                    if (found != null)
                    {
                        result.Add(found);
                        Console.WriteLine("Hockey official already existed for personId " + personId);
                        continue;
                    }
                }

                throw new HttpRequestException(
                    "Create Hockey Official failed with " + (int)response.StatusCode + ": " + body);
            }

            ApiResponse<HockeyOfficialDto>? api =
                await response.Content.ReadFromJsonAsync<ApiResponse<HockeyOfficialDto>>(jsonOptions);
            if (api?.Data == null)
            {
                throw new InvalidOperationException("Create hockey official returned empty payload.");
            }

            result.Add(api.Data);
            existing.Add(api.Data);
            Console.WriteLine("Created hockey official for personId " + personId + " (" + api.Data.Id + ")");
        }

        Console.WriteLine("Hockey officials seed: produced " + result.Count + " official(s).");
        return result;
    }

    private static async Task<List<HockeyOfficialDto>> LoadAllOfficialsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions)
    {
        HttpResponseMessage resp = await http.GetAsync("api/HockeyOfficial?isActive=true");
        if (!resp.IsSuccessStatusCode)
        {
            Console.WriteLine("Warning: list hockey officials failed: " + (int)resp.StatusCode);
            return new List<HockeyOfficialDto>();
        }

        ApiResponse<IReadOnlyList<HockeyOfficialDto>>? api =
            await resp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<HockeyOfficialDto>>>(jsonOptions);
        return api?.Data?.ToList() ?? new List<HockeyOfficialDto>();
    }
}
