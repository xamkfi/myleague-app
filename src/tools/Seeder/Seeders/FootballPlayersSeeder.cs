using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Football.Players.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace Seeder;

public static class FootballPlayersSeeder
{
    public static async Task<(List<FootballPlayerDto> players, Dictionary<string, Guid> emailToPlayerId)> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<PersonDto> playerPersons,
        List<PersonDto> goaliePersons,
        Dictionary<string, Guid> seedEmailToPersonId)
    {
        List<FootballPlayerDto> created = new List<FootballPlayerDto>();
        Dictionary<string, Guid> emailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        Dictionary<Guid, List<string>> personIdToSeedEmails = new Dictionary<Guid, List<string>>();
        foreach (KeyValuePair<string, Guid> kvp in seedEmailToPersonId)
        {
            if (!personIdToSeedEmails.ContainsKey(kvp.Value))
            {
                personIdToSeedEmails[kvp.Value] = new List<string>();
            }
            personIdToSeedEmails[kvp.Value].Add(kvp.Key);
        }

        List<FootballPlayerDto> existingPlayers = await FetchAllPlayersAsync(http, jsonOptions);
        Dictionary<Guid, FootballPlayerDto> existingByPersonId = existingPlayers
            .GroupBy(p => p.PersonId)
            .ToDictionary(g => g.Key, g => g.First());

        List<PersonDto> all = new List<PersonDto>();
        all.AddRange(playerPersons);
        all.AddRange(goaliePersons);

        foreach (PersonDto person in all)
        {
            if (existingByPersonId.TryGetValue(person.Id, out FootballPlayerDto? existing))
            {
                created.Add(existing);
                MapEmails(person.Id, existing.Id, personIdToSeedEmails, emailToPlayerId);
                Console.WriteLine("Football player exists for person, skipping: " + person.FullName + " (playerId: " + existing.Id + ")");
                continue;
            }

            CreateFootballPlayerRequest request = new CreateFootballPlayerRequest
            {
                PersonId = person.Id
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/FootballPlayer", request);
            await SeederHttp.EnsureSuccessWithBody(response, "Create Football Player");

            ApiResponse<FootballPlayerDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FootballPlayerDto>>(jsonOptions);
            if (api == null || !api.Success || api.Data == null)
            {
                throw new InvalidOperationException("Create football player failed: " + (api != null ? api.Message : "null response"));
            }

            created.Add(api.Data);
            existingByPersonId[person.Id] = api.Data;
            MapEmails(person.Id, api.Data.Id, personIdToSeedEmails, emailToPlayerId);
            Console.WriteLine("Created football player for personId " + person.Id + " (playerId: " + api.Data.Id + ")");
        }

        return (created, emailToPlayerId);
    }

    private static void MapEmails(
        Guid personId,
        Guid playerId,
        Dictionary<Guid, List<string>> personIdToSeedEmails,
        Dictionary<string, Guid> emailToPlayerId)
    {
        if (!personIdToSeedEmails.TryGetValue(personId, out List<string>? emails))
        {
            return;
        }

        foreach (string seedEmail in emails)
        {
            emailToPlayerId[seedEmail] = playerId;
        }
    }

    private static async Task<List<FootballPlayerDto>> FetchAllPlayersAsync(HttpClient http, JsonSerializerOptions jsonOptions)
    {
        List<FootballPlayerDto> all = new List<FootballPlayerDto>();
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage listResp = await http.GetAsync($"api/FootballPlayer?Page={page}&PageSize={pageSize}&IsActive=");
            if (!listResp.IsSuccessStatusCode)
            {
                break;
            }

            PaginatedApiResponse<FootballPlayerDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FootballPlayerDto>>(jsonOptions);
            if (listApi == null || !listApi.Success || listApi.Data == null)
            {
                break;
            }

            List<FootballPlayerDto> pageItems = listApi.Data.ToList();
            all.AddRange(pageItems);
            if (pageItems.Count < pageSize)
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
}
