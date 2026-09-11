using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Teams.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace Seeder;

public static class FootballTeamsSeeder
{
    public static async Task<List<FootballTeamDto>> SeedTeamsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<FootballTeamSeed> teams,
        List<DivisionDto> divisions,
        List<ClubDto> clubs)
    {
        List<FootballTeamDto> created = new List<FootballTeamDto>();
        List<FootballTeamDto> existingTeams = await FetchAllTeamsAsync(http, jsonOptions);

        foreach (FootballTeamSeed team in teams)
        {
            Guid divisionId = ResolveDivisionId(team.DivisionName, divisions);
            Guid clubId = ResolveClubId(team.ClubName, clubs);

            FootballTeamDto? existing = existingTeams.FirstOrDefault(t =>
                string.Equals(t.Name, team.Name, StringComparison.OrdinalIgnoreCase)
                && t.DivisionId == divisionId
                && t.Club != null
                && t.Club.Id == clubId);
            if (existing != null)
            {
                created.Add(existing);
                Console.WriteLine("Football team exists, skipping: " + existing.Name + " (" + existing.Id + ")");
                continue;
            }

            FootballTeamRequest request = new FootballTeamRequest
            {
                Name = team.Name,
                DivisionId = divisionId,
                ClubId = clubId,
                HomeArena = team.HomeArena,
                PrimaryJerseyColor = team.PrimaryJerseyColor,
                SecondaryJerseyColor = team.SecondaryJerseyColor,
                Category = team.Category
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/FootballTeam", request);
            await SeederHttp.EnsureSuccessWithBody(response, "Create Football Team");

            ApiResponse<FootballTeamDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FootballTeamDto>>(jsonOptions);
            if (api == null || !api.Success || api.Data == null)
            {
                throw new InvalidOperationException("Create football team failed: " + (api != null ? api.Message : "null response"));
            }

            created.Add(api.Data);
            existingTeams.Add(api.Data);
            Console.WriteLine("Created football team " + api.Data.Name + " (" + api.Data.Id + ")");
        }

        return created;
    }

    public static async Task AssignTeamsToSeasonsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<FootballSeasonDto> seasons,
        List<FootballSeasonSeed> seasonSeeds,
        List<FootballTeamSeed> teamSeeds,
        List<FootballTeamDto> teams,
        List<DivisionDto> divisions)
    {
        foreach (FootballSeasonSeed seasonSeed in seasonSeeds)
        {
            FootballSeasonDto? season = seasons.FirstOrDefault(s => string.Equals(s.Name, seasonSeed.Name, StringComparison.OrdinalIgnoreCase));
            if (season == null)
            {
                continue;
            }

            HashSet<Guid> seasonDivisionIds = new HashSet<Guid>();
            foreach (string divisionName in seasonSeed.DivisionNames)
            {
                seasonDivisionIds.Add(ResolveDivisionId(divisionName, divisions));
            }

            foreach (FootballTeamSeed teamSeed in teamSeeds)
            {
                Guid teamDivisionId = ResolveDivisionId(teamSeed.DivisionName, divisions);
                if (!seasonDivisionIds.Contains(teamDivisionId))
                {
                    continue;
                }

                FootballTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
                if (team == null)
                {
                    continue;
                }

                HttpResponseMessage resp = await http.PostAsync(
                    "api/FootballSeason/" + season.Id + "/divisions/" + teamDivisionId + "/teams/" + team.Id,
                    null);
                await SeederHttp.EnsureSuccess(resp, "Assign Football Team to Season Division");
                Console.WriteLine("Assigned football team " + team.Name + " to season " + season.Name + " division " + teamSeed.DivisionName);
            }
        }
    }

    public static async Task AddPlayersAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid teamId,
        List<FootballTeamPlayerByEmailSeed> players,
        Dictionary<string, Guid> emailToPlayerId)
    {
        HashSet<int> existingJerseyNumbers = new HashSet<int>();
        HashSet<Guid> existingPlayerIds = new HashSet<Guid>();
        HttpResponseMessage teamResp = await http.GetAsync("api/FootballTeam/" + teamId);
        if (teamResp.IsSuccessStatusCode)
        {
            ApiResponse<FootballTeamDto>? teamApi = await teamResp.Content.ReadFromJsonAsync<ApiResponse<FootballTeamDto>>(jsonOptions);
            if (teamApi != null && teamApi.Success && teamApi.Data != null && teamApi.Data.Roster != null)
            {
                foreach (FootballTeamPlayerDto rosterPlayer in teamApi.Data.Roster)
                {
                    if (rosterPlayer.JerseyNumber.HasValue)
                    {
                        existingJerseyNumbers.Add(rosterPlayer.JerseyNumber.Value);
                    }
                    existingPlayerIds.Add(rosterPlayer.PlayerId);
                }
            }
        }

        foreach (FootballTeamPlayerByEmailSeed player in players)
        {
            if (!emailToPlayerId.TryGetValue(player.PersonEmail, out Guid playerId))
            {
                HttpResponseMessage personResp = await http.GetAsync("api/persons/by-email?email=" + Uri.EscapeDataString(player.PersonEmail));
                if (!personResp.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("No person found for email: " + player.PersonEmail);
                }

                ApiResponse<PersonDto>? personApi = await personResp.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(jsonOptions);
                if (personApi == null || !personApi.Success || personApi.Data == null)
                {
                    throw new InvalidOperationException("Failed to fetch person for email: " + player.PersonEmail);
                }

                Guid personId = personApi.Data.Id;
                playerId = await EnsurePlayerForPersonAsync(http, jsonOptions, personId, player.PersonEmail, emailToPlayerId);
            }

            if (existingPlayerIds.Contains(playerId))
            {
                continue;
            }

            if (existingJerseyNumbers.Contains(player.JerseyNumber))
            {
                Console.WriteLine("Jersey number already in use (" + player.JerseyNumber + ") for football team " + teamId + ", skipping " + player.PersonEmail);
                continue;
            }

            int positionValue = (int)player.Position;
            HttpResponseMessage response = await http.PostAsync(
                $"api/FootballTeam/{teamId}/players/{playerId}?position={positionValue}&jerseyNumber={player.JerseyNumber}",
                null);
            await SeederHttp.EnsureSuccessWithBody(response, "Add Player To Football Team");

            existingJerseyNumbers.Add(player.JerseyNumber);
            existingPlayerIds.Add(playerId);
        }
    }

    private static async Task<Guid> EnsurePlayerForPersonAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid personId,
        string personEmail,
        Dictionary<string, Guid> emailToPlayerId)
    {
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage listResp = await http.GetAsync($"api/FootballPlayer?Page={page}&PageSize={pageSize}&IsActive=");
            if (listResp.IsSuccessStatusCode)
            {
                PaginatedApiResponse<FootballPlayerDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FootballPlayerDto>>(jsonOptions);
                if (listApi != null && listApi.Success && listApi.Data != null)
                {
                    FootballPlayerDto? existing = listApi.Data.FirstOrDefault(p => p.PersonId == personId);
                    if (existing != null)
                    {
                        emailToPlayerId[personEmail] = existing.Id;
                        return existing.Id;
                    }

                    if (listApi.Data.Count() < pageSize)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }

            page++;
            if (page > 50)
            {
                break;
            }
        }

        CreateFootballPlayerRequest createReq = new CreateFootballPlayerRequest { PersonId = personId };
        HttpResponseMessage createResp = await http.PostAsJsonAsync("api/FootballPlayer", createReq);
        await SeederHttp.EnsureSuccessWithBody(createResp, "Create Football Player (fallback)");

        ApiResponse<FootballPlayerDto>? createApi = await createResp.Content.ReadFromJsonAsync<ApiResponse<FootballPlayerDto>>(jsonOptions);
        if (createApi == null || !createApi.Success || createApi.Data == null)
        {
            throw new InvalidOperationException("Create football player (fallback) failed for email: " + personEmail);
        }

        emailToPlayerId[personEmail] = createApi.Data.Id;
        return createApi.Data.Id;
    }

    private static async Task<List<FootballTeamDto>> FetchAllTeamsAsync(HttpClient http, JsonSerializerOptions jsonOptions)
    {
        List<FootballTeamDto> all = new List<FootballTeamDto>();
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage listResp = await http.GetAsync($"api/FootballTeam?Page={page}&PageSize={pageSize}");
            if (!listResp.IsSuccessStatusCode)
            {
                break;
            }

            PaginatedApiResponse<FootballTeamDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FootballTeamDto>>(jsonOptions);
            if (listApi == null || !listApi.Success || listApi.Data == null)
            {
                break;
            }

            List<FootballTeamDto> pageItems = listApi.Data.ToList();
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

    private static Guid ResolveDivisionId(string divisionName, List<DivisionDto> divisions)
    {
        DivisionDto? division = divisions.FirstOrDefault(d => string.Equals(d.Name, divisionName, StringComparison.OrdinalIgnoreCase));
        if (division == null)
        {
            throw new InvalidOperationException("Division not found by name: " + divisionName);
        }
        return division.Id;
    }

    private static Guid ResolveClubId(string clubName, List<ClubDto> clubs)
    {
        ClubDto? club = clubs.FirstOrDefault(c => string.Equals(c.Name, clubName, StringComparison.OrdinalIgnoreCase));
        if (club == null)
        {
            throw new InvalidOperationException("Club not found by name: " + clubName);
        }
        return club.Id;
    }
}
