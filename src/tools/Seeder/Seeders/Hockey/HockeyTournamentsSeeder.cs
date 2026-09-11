using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

public static class HockeyTournamentsSeeder
{
    public static async Task<List<HockeyTournamentDto>> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<HockeyTournamentSeed> tournaments,
        List<HockeyTeamDto> teams)
    {
        List<HockeyTournamentDto> created = new List<HockeyTournamentDto>();

        foreach (HockeyTournamentSeed tournamentSeed in tournaments)
        {
            HockeyTournamentDto tournament = await FindOrCreateTournamentAsync(http, jsonOptions, tournamentSeed);

            foreach (string teamName in tournamentSeed.Groups.SelectMany(g => g.TeamNames).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                Guid teamId = ResolveTeamId(teamName, teams);
                tournament = await EnsureCompetitionTeamAsync(http, jsonOptions, tournament, teamId, teamName);
            }

            foreach (HockeyTournamentGroupSeed groupSeed in tournamentSeed.Groups)
            {
                tournament = await EnsureGroupAsync(http, jsonOptions, tournament, groupSeed);

                HockeyTournamentGroupDto? group = tournament.Groups.FirstOrDefault(g =>
                    string.Equals(g.Name, groupSeed.Name, StringComparison.OrdinalIgnoreCase));
                if (group == null)
                {
                    throw new InvalidOperationException("Group '" + groupSeed.Name + "' not present after creation in tournament '" + tournament.Name + "'.");
                }

                foreach (string teamName in groupSeed.TeamNames)
                {
                    Guid teamId = ResolveTeamId(teamName, teams);
                    HockeyCompetitionTeamDto? competitionTeam = tournament.Teams.FirstOrDefault(t => t.TeamId == teamId && t.IsActive);
                    if (competitionTeam == null)
                    {
                        throw new InvalidOperationException("Competition team missing for '" + teamName + "' in tournament '" + tournament.Name + "'.");
                    }

                    if (group.Teams.Any(t => t.CompetitionTeamId == competitionTeam.Id && t.IsActive))
                    {
                        Console.WriteLine("Team '" + teamName + "' already in group '" + group.Name + "' of tournament '" + tournament.Name + "', skipping");
                        continue;
                    }

                    AddTeamToHockeyTournamentGroupRequest addTeamReq = new AddTeamToHockeyTournamentGroupRequest
                    {
                        CompetitionTeamId = competitionTeam.Id
                    };
                    HttpResponseMessage addTeamResp = await http.PostAsJsonAsync(
                        "api/HockeyTournament/" + tournament.Id + "/groups/" + group.Id + "/teams",
                        addTeamReq);
                    await SeederHttp.EnsureSuccessWithBody(addTeamResp, "Add Team To Hockey Tournament Group");

                    ApiResponse<HockeyTournamentDto>? addTeamApi =
                        await addTeamResp.Content.ReadFromJsonAsync<ApiResponse<HockeyTournamentDto>>(jsonOptions);
                    if (addTeamApi?.Data == null)
                    {
                        throw new InvalidOperationException("Add team to hockey tournament group failed.");
                    }

                    tournament = addTeamApi.Data;
                    group = tournament.Groups.First(g => g.Id == group.Id);
                    Console.WriteLine("Added team '" + teamName + "' to group '" + group.Name + "' of tournament '" + tournament.Name + "'");
                }
            }

            tournament = await EnsurePublishedAsync(http, jsonOptions, tournament);
            tournament = await TryAdvanceToGroupStageAsync(http, jsonOptions, tournament);

            created.Add(tournament);
            Console.WriteLine("Hockey tournament ready: " + tournament.Name + " (" + tournament.Id + ") [status: " + tournament.Status + ", stage: " + tournament.CurrentStage + "]");
        }

        return created;
    }

    private static async Task<HockeyTournamentDto> FindOrCreateTournamentAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyTournamentSeed seed)
    {
        (DateTime startUtc, DateTime endUtc) = ComputeTournamentWindowUtc();

        HttpResponseMessage listResp = await http.GetAsync("api/HockeyTournament");
        if (listResp.IsSuccessStatusCode)
        {
            ApiResponse<List<HockeyTournamentDto>>? listApi =
                await listResp.Content.ReadFromJsonAsync<ApiResponse<List<HockeyTournamentDto>>>(jsonOptions);
            HockeyTournamentDto? existing = listApi?.Data?.FirstOrDefault(t =>
                string.Equals(t.Name, seed.Name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                Console.WriteLine("Hockey tournament exists: " + existing.Name + " (" + existing.Id + ")");
                return await GetByIdAsync(http, jsonOptions, existing.Id) ?? existing;
            }
        }

        CreateHockeyTournamentRequest request = new CreateHockeyTournamentRequest
        {
            Name = seed.Name,
            StartDate = startUtc,
            EndDate = endUtc,
            Venue = seed.Venue,
            ContentHtml = seed.ContentHtml,
            TeamCategory = seed.TeamCategory
        };

        HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyTournament", request);
        await SeederHttp.EnsureSuccessWithBody(response, "Create Hockey Tournament");

        ApiResponse<HockeyTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeyTournamentDto>>(jsonOptions);
        if (api?.Data == null)
        {
            throw new InvalidOperationException("Create hockey tournament failed.");
        }

        Console.WriteLine("Created hockey tournament " + api.Data.Name + " (" + api.Data.Id + ")");
        return api.Data;
    }

    private static async Task<HockeyTournamentDto> EnsureCompetitionTeamAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyTournamentDto tournament,
        Guid teamId,
        string teamName)
    {
        if (tournament.Teams.Any(t => t.TeamId == teamId && t.IsActive))
        {
            return tournament;
        }

        AddTeamToHockeyCompetitionRequest request = new AddTeamToHockeyCompetitionRequest { TeamId = teamId };
        HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyTournament/" + tournament.Id + "/teams", request);
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Note: add team to tournament returned " + (int)response.StatusCode + " for " + teamName + ": " + Truncate(body));
            HockeyTournamentDto? refreshed = await GetByIdAsync(http, jsonOptions, tournament.Id);
            if (refreshed != null && refreshed.Teams.Any(t => t.TeamId == teamId && t.IsActive))
            {
                return refreshed;
            }
            await SeederHttp.EnsureSuccessWithBody(response, "Add Team To Hockey Tournament");
        }

        ApiResponse<HockeyCompetitionTeamDto>? api =
            await response.Content.ReadFromJsonAsync<ApiResponse<HockeyCompetitionTeamDto>>(jsonOptions);
        Console.WriteLine("Added team '" + teamName + "' to tournament '" + tournament.Name + "'");
        return await GetByIdAsync(http, jsonOptions, tournament.Id) ?? tournament;
    }

    private static async Task<HockeyTournamentDto> EnsureGroupAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyTournamentDto tournament,
        HockeyTournamentGroupSeed groupSeed)
    {
        if (tournament.Groups.Any(g => string.Equals(g.Name, groupSeed.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Group '" + groupSeed.Name + "' already exists in tournament '" + tournament.Name + "', skipping");
            return tournament;
        }

        CreateHockeyTournamentGroupRequest request = new CreateHockeyTournamentGroupRequest { Name = groupSeed.Name };
        HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyTournament/" + tournament.Id + "/groups", request);
        await SeederHttp.EnsureSuccessWithBody(response, "Add Group To Hockey Tournament");

        ApiResponse<HockeyTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeyTournamentDto>>(jsonOptions);
        if (api?.Data == null)
        {
            return await GetByIdAsync(http, jsonOptions, tournament.Id) ?? tournament;
        }

        Console.WriteLine("Created group '" + groupSeed.Name + "' in tournament '" + tournament.Name + "'");
        return api.Data;
    }

    private static async Task<HockeyTournamentDto> EnsurePublishedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyTournamentDto tournament)
    {
        if (!string.Equals(tournament.Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            return tournament;
        }

        HttpResponseMessage response = await http.PostAsync("api/HockeyTournament/" + tournament.Id + "/publish", content: null);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Warning: publish hockey tournament failed: " + await response.Content.ReadAsStringAsync());
            return await GetByIdAsync(http, jsonOptions, tournament.Id) ?? tournament;
        }

        ApiResponse<HockeyTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeyTournamentDto>>(jsonOptions);
        return api?.Data ?? await GetByIdAsync(http, jsonOptions, tournament.Id) ?? tournament;
    }

    private static async Task<HockeyTournamentDto> TryAdvanceToGroupStageAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyTournamentDto tournament)
    {
        if (!string.Equals(tournament.CurrentStage, "Registration", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Tournament '" + tournament.Name + "' already in stage '" + tournament.CurrentStage + "', skipping start-group-stage");
            return tournament;
        }

        HttpResponseMessage response = await http.PostAsync(
            "api/HockeyTournament/" + tournament.Id + "/start-group-stage",
            content: null);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Warning: failed to advance tournament '" + tournament.Name + "' to GroupStage (" + (int)response.StatusCode + "): " + body);
            return tournament;
        }

        ApiResponse<HockeyTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeyTournamentDto>>(jsonOptions);
        if (api?.Data == null)
        {
            return await GetByIdAsync(http, jsonOptions, tournament.Id) ?? tournament;
        }

        Console.WriteLine("Advanced tournament '" + tournament.Name + "' to stage '" + api.Data.CurrentStage + "' / status '" + api.Data.Status + "'");
        return api.Data;
    }

    private static async Task<HockeyTournamentDto?> GetByIdAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid id)
    {
        HttpResponseMessage resp = await http.GetAsync("api/HockeyTournament/" + id);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<HockeyTournamentDto>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<HockeyTournamentDto>>(jsonOptions);
        return api?.Data;
    }

    private static (DateTime StartUtc, DateTime EndUtc) ComputeTournamentWindowUtc()
    {
        DateTime start = DateTime.UtcNow.Date.AddDays(-6);
        DateTime end = DateTime.UtcNow.Date.AddDays(12);
        return (start, end);
    }

    private static Guid ResolveTeamId(string teamName, List<HockeyTeamDto> teams)
    {
        HockeyTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
        if (team == null)
        {
            throw new InvalidOperationException("Hockey team not found by name: " + teamName);
        }
        return team.Id;
    }

    private static string Truncate(string body) => body.Length > 300 ? body.Substring(0, 300) + "..." : body;
}
