using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

/// <summary>
/// Creates hockey player profiles for persons. There is no player list API, so idempotency
/// resolves existing players via team rosters (<c>GET api/HockeyTeam</c> → player → person).
/// </summary>
public static class HockeyPlayersSeeder
{
    public static async Task<(List<HockeyPlayerDto> Players, Dictionary<string, Guid> EmailToPlayerId)> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<PersonDto> persons,
        Dictionary<string, Guid> seedEmailToPersonId,
        IReadOnlyList<HockeyTeamSeed> teamSeeds)
    {
        List<HockeyPlayerDto> created = new List<HockeyPlayerDto>();
        Dictionary<string, Guid> emailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        Dictionary<Guid, List<string>> personIdToSeedEmails = new Dictionary<Guid, List<string>>();
        foreach (KeyValuePair<string, Guid> kvp in seedEmailToPersonId)
        {
            if (!personIdToSeedEmails.TryGetValue(kvp.Value, out List<string>? emails))
            {
                emails = new List<string>();
                personIdToSeedEmails[kvp.Value] = emails;
            }
            emails.Add(kvp.Key);
        }

        Dictionary<Guid, HockeyPosition> preferredPositionByPersonId = BuildPreferredPositions(teamSeeds, seedEmailToPersonId);

        Dictionary<Guid, HockeyPlayerDto> existingByPersonId = await LoadExistingPlayersByPersonIdAsync(http, jsonOptions);

        foreach (PersonDto person in persons)
        {
            if (existingByPersonId.TryGetValue(person.Id, out HockeyPlayerDto? existing))
            {
                created.Add(existing);
                MapEmails(personIdToSeedEmails, person.Id, existing.Id, emailToPlayerId);
                Console.WriteLine("Hockey player exists for person, skipping: " + person.FullName + " (playerId: " + existing.Id + ")");
                continue;
            }

            HockeyPosition position = preferredPositionByPersonId.TryGetValue(person.Id, out HockeyPosition preferred)
                ? preferred
                : HockeyPosition.Center;

            CreateHockeyPlayerRequest request = new CreateHockeyPlayerRequest
            {
                PersonId = person.Id,
                PrimaryPosition = position,
                Shoots = HockeyShoots.Unknown,
                Catches = position == HockeyPosition.Goalie ? HockeyCatches.Unknown : null
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyPlayer", request);
            if (!response.IsSuccessStatusCode)
            {
                // Re-scan in case another phase/run created the player between our check and POST.
                existingByPersonId = await LoadExistingPlayersByPersonIdAsync(http, jsonOptions);
                if (existingByPersonId.TryGetValue(person.Id, out HockeyPlayerDto? raced))
                {
                    created.Add(raced);
                    MapEmails(personIdToSeedEmails, person.Id, raced.Id, emailToPlayerId);
                    Console.WriteLine("Hockey player exists after create race, using: " + person.FullName + " (playerId: " + raced.Id + ")");
                    continue;
                }

                await SeederHttp.EnsureSuccessWithBody(response, "Create Hockey Player");
            }

            ApiResponse<HockeyPlayerDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeyPlayerDto>>(jsonOptions);
            if (api == null || !api.Success || api.Data == null)
            {
                throw new InvalidOperationException("Create hockey player failed: " + (api != null ? api.Message : "null response"));
            }

            created.Add(api.Data);
            existingByPersonId[person.Id] = api.Data;
            MapEmails(personIdToSeedEmails, person.Id, api.Data.Id, emailToPlayerId);
            Console.WriteLine("Created hockey player for personId " + person.Id + " (playerId: " + api.Data.Id + ")");
        }

        return (created, emailToPlayerId);
    }

    private static Dictionary<Guid, HockeyPosition> BuildPreferredPositions(
        IReadOnlyList<HockeyTeamSeed> teamSeeds,
        Dictionary<string, Guid> seedEmailToPersonId)
    {
        Dictionary<Guid, HockeyPosition> map = new Dictionary<Guid, HockeyPosition>();
        foreach (HockeyTeamSeed team in teamSeeds)
        {
            foreach (HockeyTeamPlayerByEmailSeed player in team.Players)
            {
                if (seedEmailToPersonId.TryGetValue(player.PersonEmail, out Guid personId))
                {
                    map[personId] = player.Position;
                }
            }
        }
        return map;
    }

    private static void MapEmails(
        Dictionary<Guid, List<string>> personIdToSeedEmails,
        Guid personId,
        Guid playerId,
        Dictionary<string, Guid> emailToPlayerId)
    {
        if (!personIdToSeedEmails.TryGetValue(personId, out List<string>? emails))
        {
            return;
        }

        foreach (string email in emails)
        {
            emailToPlayerId[email] = playerId;
        }
    }

    internal static async Task<Dictionary<Guid, HockeyPlayerDto>> LoadExistingPlayersByPersonIdAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions)
    {
        Dictionary<Guid, HockeyPlayerDto> byPersonId = new Dictionary<Guid, HockeyPlayerDto>();

        HttpResponseMessage listResp = await http.GetAsync("api/HockeyTeam");
        if (!listResp.IsSuccessStatusCode)
        {
            return byPersonId;
        }

        ApiResponse<List<HockeyTeamDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<HockeyTeamDto>>>(jsonOptions);
        if (listApi?.Data == null)
        {
            return byPersonId;
        }

        HashSet<Guid> playerIds = new HashSet<Guid>();
        foreach (HockeyTeamDto teamSummary in listApi.Data)
        {
            HttpResponseMessage detailResp = await http.GetAsync("api/HockeyTeam/" + teamSummary.Id);
            if (!detailResp.IsSuccessStatusCode)
            {
                continue;
            }

            ApiResponse<HockeyTeamDto>? detailApi = await detailResp.Content.ReadFromJsonAsync<ApiResponse<HockeyTeamDto>>(jsonOptions);
            if (detailApi?.Data?.Roster == null)
            {
                continue;
            }

            foreach (HockeyTeamPlayerDto rosterPlayer in detailApi.Data.Roster)
            {
                playerIds.Add(rosterPlayer.PlayerId);
            }
        }

        foreach (Guid playerId in playerIds)
        {
            HttpResponseMessage playerResp = await http.GetAsync("api/HockeyPlayer/" + playerId);
            if (!playerResp.IsSuccessStatusCode)
            {
                continue;
            }

            ApiResponse<HockeyPlayerDto>? playerApi = await playerResp.Content.ReadFromJsonAsync<ApiResponse<HockeyPlayerDto>>(jsonOptions);
            if (playerApi?.Data != null)
            {
                byPersonId[playerApi.Data.PersonId] = playerApi.Data;
            }
        }

        return byPersonId;
    }
}
