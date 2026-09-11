using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Tournaments.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Football;

namespace Seeder;

public static class FootballTournamentsSeeder
{
    public static async Task<List<FootballTournamentDto>> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<FootballTournamentSeed> tournaments,
        List<FootballTeamDto> teams)
    {
        List<FootballTournamentDto> created = new List<FootballTournamentDto>();

        foreach (FootballTournamentSeed tournamentSeed in tournaments)
        {
            FootballTournamentDto tournament = await FindOrCreateTournamentAsync(http, jsonOptions, tournamentSeed);

            foreach (FootballTournamentGroupSeed groupSeed in tournamentSeed.Groups)
            {
                tournament = await EnsureGroupAsync(http, jsonOptions, tournament, groupSeed);

                FootballTournamentGroupDto? group = tournament.Groups.FirstOrDefault(g => string.Equals(g.Name, groupSeed.Name, StringComparison.OrdinalIgnoreCase));
                if (group == null)
                {
                    throw new InvalidOperationException($"Group '{groupSeed.Name}' not present after creation in tournament '{tournament.Name}'.");
                }

                foreach (string teamName in groupSeed.TeamNames)
                {
                    Guid teamId = ResolveTeamId(teamName, teams);

                    if (group.Teams.Any(t => t.TeamId == teamId))
                    {
                        Console.WriteLine($"Team '{teamName}' already in group '{group.Name}' of football tournament '{tournament.Name}', skipping");
                        continue;
                    }

                    AddTeamToTournamentGroupRequest addTeamReq = new AddTeamToTournamentGroupRequest { TeamId = teamId };
                    HttpResponseMessage addTeamResp = await http.PostAsJsonAsync($"api/FootballTournament/{tournament.Id}/groups/{group.Id}/teams", addTeamReq);
                    await SeederHttp.EnsureSuccessWithBody(addTeamResp, "Add Team To Football Tournament Group");

                    ApiResponse<FootballTournamentDto>? addTeamApi = await addTeamResp.Content.ReadFromJsonAsync<ApiResponse<FootballTournamentDto>>(jsonOptions);
                    if (addTeamApi == null || !addTeamApi.Success || addTeamApi.Data == null)
                    {
                        throw new InvalidOperationException($"Add team to football tournament group failed: {(addTeamApi != null ? addTeamApi.Message : "null response")}");
                    }

                    tournament = addTeamApi.Data;
                    group = tournament.Groups.First(g => g.Id == group.Id);
                    Console.WriteLine($"Added team '{teamName}' to group '{group.Name}' of football tournament '{tournament.Name}'");
                }
            }

            if (tournamentSeed.AllGroupMatchesCompleted)
            {
                tournament = await TryAdvanceToGroupStageAsync(http, jsonOptions, tournament);
            }

            created.Add(tournament);
            Console.WriteLine($"Football tournament ready: {tournament.Name} ({tournament.Id}) with {tournament.Groups.Count} group(s) and {tournament.TeamCount} team(s) [status: {tournament.TournamentStatus}]");
        }

        return created;
    }

    private static async Task<FootballTournamentDto> TryAdvanceToGroupStageAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        FootballTournamentDto tournament)
    {
        if (!string.Equals(tournament.TournamentStatus, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"  Football tournament '{tournament.Name}' already in status '{tournament.TournamentStatus}', skipping start-group-stage");
            return tournament;
        }

        HttpResponseMessage response = await http.PostAsync(
            $"api/FootballTournament/{tournament.Id}/start-group-stage",
            content: null);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"  Warning: failed to advance football tournament '{tournament.Name}' to GroupStage ({(int)response.StatusCode}): {body}");
            return tournament;
        }

        ApiResponse<FootballTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FootballTournamentDto>>(jsonOptions);
        if (api?.Data == null)
        {
            Console.WriteLine($"  Warning: start-group-stage returned an empty payload for '{tournament.Name}'.");
            return tournament;
        }

        Console.WriteLine($"  Advanced football tournament '{tournament.Name}' to status '{api.Data.TournamentStatus}'");
        return api.Data;
    }

    private static async Task<FootballTournamentDto> FindOrCreateTournamentAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        FootballTournamentSeed seed)
    {
        (DateTime startUtc, DateTime endUtc) = ComputeTournamentWindowUtc();

        HttpResponseMessage listResp = await http.GetAsync("api/FootballTournament");
        if (listResp.IsSuccessStatusCode)
        {
            ApiResponse<List<FootballTournamentDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<FootballTournamentDto>>>(jsonOptions);
            if (listApi != null && listApi.Success && listApi.Data != null)
            {
                FootballTournamentDto? existing = listApi.Data.FirstOrDefault(t => string.Equals(t.Name, seed.Name, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    Console.WriteLine($"Football tournament exists, refreshing dates: {existing.Name} ({existing.Id})");
                    await TryRefreshTournamentDatesAsync(http, jsonOptions, existing, seed, startUtc, endUtc);

                    HttpResponseMessage detailResp = await http.GetAsync($"api/FootballTournament/{existing.Id}");
                    if (detailResp.IsSuccessStatusCode)
                    {
                        ApiResponse<FootballTournamentDto>? detailApi = await detailResp.Content.ReadFromJsonAsync<ApiResponse<FootballTournamentDto>>(jsonOptions);
                        if (detailApi != null && detailApi.Success && detailApi.Data != null)
                        {
                            return detailApi.Data;
                        }
                    }
                    return existing;
                }
            }
        }

        CreateFootballTournamentRequest request = new CreateFootballTournamentRequest
        {
            Name = seed.Name,
            StartDate = startUtc,
            EndDate = endUtc,
            Venue = seed.Venue,
            ContentHtml = seed.ContentHtml,
            GroupStageNumberOfHalves = seed.GroupStageNumberOfHalves,
            GroupStageHalfDurationMinutes = seed.GroupStageHalfDurationMinutes,
            GroupStagePlayersOnField = seed.GroupStagePlayersOnField,
            GroupStageRequireGoalkeeper = seed.GroupStageRequireGoalkeeper,
            GroupStageMaxSubstitutions = seed.GroupStageMaxSubstitutions,
            GroupStageRequireOfficialsToStart = seed.GroupStageRequireOfficialsToStart,
            GroupStageAllowExtraTime = seed.GroupStageAllowExtraTime,
            GroupStageExtraTimeHalfCount = seed.GroupStageExtraTimeHalfCount,
            GroupStageExtraTimeHalfDurationMinutes = seed.GroupStageExtraTimeHalfDurationMinutes,
            GroupStageAllowPenaltyShootout = seed.GroupStageAllowPenaltyShootout,
            PlayoffNumberOfHalves = seed.PlayoffNumberOfHalves,
            PlayoffHalfDurationMinutes = seed.PlayoffHalfDurationMinutes,
            PlayoffPlayersOnField = seed.PlayoffPlayersOnField,
            PlayoffRequireGoalkeeper = seed.PlayoffRequireGoalkeeper,
            PlayoffMaxSubstitutions = seed.PlayoffMaxSubstitutions,
            PlayoffRequireOfficialsToStart = seed.PlayoffRequireOfficialsToStart,
            PlayoffAllowExtraTime = seed.PlayoffAllowExtraTime,
            PlayoffExtraTimeHalfCount = seed.PlayoffExtraTimeHalfCount,
            PlayoffExtraTimeHalfDurationMinutes = seed.PlayoffExtraTimeHalfDurationMinutes,
            PlayoffAllowPenaltyShootout = seed.PlayoffAllowPenaltyShootout,
            TeamsAdvancingPerGroup = seed.TeamsAdvancingPerGroup,
            HasPlayoffStage = seed.HasPlayoffStage,
            HasThirdPlaceMatch = seed.HasThirdPlaceMatch,
            TeamCategory = seed.TeamCategory
        };

        HttpResponseMessage response = await http.PostAsJsonAsync("api/FootballTournament", request);
        await SeederHttp.EnsureSuccessWithBody(response, "Create Football Tournament");

        ApiResponse<FootballTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FootballTournamentDto>>(jsonOptions);
        if (api == null || !api.Success || api.Data == null)
        {
            throw new InvalidOperationException("Create football tournament failed: " + (api != null ? api.Message : "null response"));
        }

        Console.WriteLine($"Created football tournament {api.Data.Name} ({api.Data.Id})");
        return api.Data;
    }

    private static async Task<FootballTournamentDto> EnsureGroupAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        FootballTournamentDto tournament,
        FootballTournamentGroupSeed groupSeed)
    {
        if (tournament.Groups.Any(g => string.Equals(g.Name, groupSeed.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"Group '{groupSeed.Name}' already exists in football tournament '{tournament.Name}', skipping");
            return tournament;
        }

        AddGroupToTournamentRequest request = new AddGroupToTournamentRequest { GroupName = groupSeed.Name };
        HttpResponseMessage response = await http.PostAsJsonAsync($"api/FootballTournament/{tournament.Id}/groups", request);
        await SeederHttp.EnsureSuccessWithBody(response, "Add Group To Football Tournament");

        ApiResponse<FootballTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FootballTournamentDto>>(jsonOptions);
        if (api == null || !api.Success || api.Data == null)
        {
            throw new InvalidOperationException("Add group to football tournament failed: " + (api != null ? api.Message : "null response"));
        }

        Console.WriteLine($"Added group '{groupSeed.Name}' to football tournament '{tournament.Name}'");
        return api.Data;
    }

    private static Guid ResolveTeamId(string teamName, List<FootballTeamDto> teams)
    {
        FootballTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
        if (team == null)
        {
            throw new InvalidOperationException($"Football team not found by name: {teamName}");
        }
        return team.Id;
    }

    private static (DateTime StartUtc, DateTime EndUtc) ComputeTournamentWindowUtc()
    {
        DateTime nowUtc = DateTime.UtcNow.Date;
        DateTime startUtc = DateTime.SpecifyKind(nowUtc.AddDays(-7), DateTimeKind.Utc);
        DateTime endUtc = DateTime.SpecifyKind(nowUtc.AddDays(14), DateTimeKind.Utc);
        return (startUtc, endUtc);
    }

    private static async Task TryRefreshTournamentDatesAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        FootballTournamentDto existing,
        FootballTournamentSeed seed,
        DateTime startUtc,
        DateTime endUtc)
    {
        bool startsClose = Math.Abs((existing.StartDate.ToUniversalTime() - startUtc).TotalDays) < 1.0;
        bool endsClose = Math.Abs((existing.EndDate.ToUniversalTime() - endUtc).TotalDays) < 1.0;
        if (startsClose && endsClose)
        {
            return;
        }

        UpdateFootballTournamentRequest update = new UpdateFootballTournamentRequest
        {
            Name = existing.Name,
            StartDate = startUtc,
            EndDate = endUtc,
            Venue = existing.Venue,
            ContentHtml = existing.ContentHtml,
            GroupStageNumberOfHalves = seed.GroupStageNumberOfHalves,
            GroupStageHalfDurationMinutes = seed.GroupStageHalfDurationMinutes,
            GroupStagePlayersOnField = seed.GroupStagePlayersOnField,
            GroupStageRequireGoalkeeper = seed.GroupStageRequireGoalkeeper,
            GroupStageMaxSubstitutions = seed.GroupStageMaxSubstitutions,
            GroupStageRequireOfficialsToStart = seed.GroupStageRequireOfficialsToStart,
            GroupStageAllowExtraTime = seed.GroupStageAllowExtraTime,
            GroupStageExtraTimeHalfCount = seed.GroupStageExtraTimeHalfCount,
            GroupStageExtraTimeHalfDurationMinutes = seed.GroupStageExtraTimeHalfDurationMinutes,
            GroupStageAllowPenaltyShootout = seed.GroupStageAllowPenaltyShootout,
            PlayoffNumberOfHalves = seed.PlayoffNumberOfHalves,
            PlayoffHalfDurationMinutes = seed.PlayoffHalfDurationMinutes,
            PlayoffPlayersOnField = seed.PlayoffPlayersOnField,
            PlayoffRequireGoalkeeper = seed.PlayoffRequireGoalkeeper,
            PlayoffMaxSubstitutions = seed.PlayoffMaxSubstitutions,
            PlayoffRequireOfficialsToStart = seed.PlayoffRequireOfficialsToStart,
            PlayoffAllowExtraTime = seed.PlayoffAllowExtraTime,
            PlayoffExtraTimeHalfCount = seed.PlayoffExtraTimeHalfCount,
            PlayoffExtraTimeHalfDurationMinutes = seed.PlayoffExtraTimeHalfDurationMinutes,
            PlayoffAllowPenaltyShootout = seed.PlayoffAllowPenaltyShootout,
            TeamsAdvancingPerGroup = seed.TeamsAdvancingPerGroup,
            HasPlayoffStage = seed.HasPlayoffStage,
            HasThirdPlaceMatch = seed.HasThirdPlaceMatch,
            TeamCategory = existing.TeamCategory
        };

        try
        {
            HttpResponseMessage response = await http.PutAsJsonAsync($"api/FootballTournament/{existing.Id}", update);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"  Refreshed football tournament window to {startUtc:yyyy-MM-dd} - {endUtc:yyyy-MM-dd}");
            }
            else
            {
                string body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"  Warning: failed to refresh football tournament window ({(int)response.StatusCode}): {body}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: football tournament window refresh threw: {ex.Message}");
        }
    }
}
