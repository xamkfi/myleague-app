using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

public static class HockeyTeamsSeeder
{
    public static async Task<List<HockeyTeamDto>> SeedTeamsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<HockeyTeamSeed> teams,
        List<DivisionDto> divisions,
        List<ClubDto> clubs)
    {
        List<HockeyTeamDto> created = new List<HockeyTeamDto>();

        HttpResponseMessage listResp = await http.GetAsync("api/HockeyTeam");
        List<HockeyTeamDto> existingTeams = new List<HockeyTeamDto>();
        if (listResp.IsSuccessStatusCode)
        {
            ApiResponse<List<HockeyTeamDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<HockeyTeamDto>>>(jsonOptions);
            if (listApi?.Data != null)
            {
                existingTeams.AddRange(listApi.Data);
            }
        }

        foreach (HockeyTeamSeed team in teams)
        {
            Guid? divisionId = string.IsNullOrWhiteSpace(team.DivisionName)
                ? null
                : ResolveDivisionId(team.DivisionName, divisions);
            Guid clubId = ResolveClubId(team.ClubName, clubs);

            HockeyTeamDto? existing = existingTeams.FirstOrDefault(t =>
                string.Equals(t.Name, team.Name, StringComparison.OrdinalIgnoreCase) &&
                t.ClubId == clubId &&
                t.DivisionId == divisionId);

            if (existing != null)
            {
                created.Add(existing);
                Console.WriteLine("Hockey team exists, skipping: " + existing.Name + " (" + existing.Id + ")");
                continue;
            }

            CreateHockeyTeamRequest request = new CreateHockeyTeamRequest
            {
                Name = team.Name,
                ShortName = team.ShortName,
                ClubId = clubId,
                DivisionId = divisionId,
                HomeArena = team.HomeArena,
                PrimaryJerseyColor = team.PrimaryJerseyColor,
                SecondaryJerseyColor = team.SecondaryJerseyColor,
                TeamCategory = team.Category
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyTeam", request);
            await SeederHttp.EnsureSuccessWithBody(response, "Create Hockey Team");

            ApiResponse<HockeyTeamDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeyTeamDto>>(jsonOptions);
            if (api == null || !api.Success || api.Data == null)
            {
                throw new InvalidOperationException("Create hockey team failed: " + (api != null ? api.Message : "null response"));
            }

            created.Add(api.Data);
            existingTeams.Add(api.Data);
            Console.WriteLine("Created hockey team " + api.Data.Name + " (" + api.Data.Id + ")");
        }

        return created;
    }

    public static async Task AssignTeamsToSeasonsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<HockeySeasonDto> seasons,
        List<HockeyTeamSeed> teamSeeds,
        List<HockeyTeamDto> teams,
        List<DivisionDto> divisions)
    {
        foreach (HockeySeasonSeed seasonSeed in Program.Configuration.HockeySeasons)
        {
            HockeySeasonDto? season = seasons.FirstOrDefault(s => string.Equals(s.Name, seasonSeed.Name, StringComparison.OrdinalIgnoreCase));
            if (season == null)
            {
                continue;
            }

            // Refresh full season so Divisions/Teams collections are populated.
            season = await GetSeasonByIdAsync(http, jsonOptions, season.Id) ?? season;

            HashSet<Guid> seasonDivisionIds = new HashSet<Guid>();
            foreach (string divisionName in seasonSeed.DivisionNames)
            {
                seasonDivisionIds.Add(ResolveDivisionId(divisionName, divisions));
            }

            foreach (HockeyTeamSeed teamSeed in teamSeeds)
            {
                Guid teamDivisionId = ResolveDivisionId(teamSeed.DivisionName, divisions);
                if (!seasonDivisionIds.Contains(teamDivisionId))
                {
                    continue;
                }

                HockeyTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
                if (team == null)
                {
                    continue;
                }

                HockeyCompetitionDivisionDto? competitionDivision = season.Divisions
                    .FirstOrDefault(d => d.DivisionId == teamDivisionId && d.IsActive);
                if (competitionDivision == null)
                {
                    Console.WriteLine("Warning: season division link missing for " + teamSeed.DivisionName + " in " + season.Name);
                    continue;
                }

                HockeyCompetitionTeamDto? competitionTeam = season.Teams.FirstOrDefault(t => t.TeamId == team.Id && t.IsActive);
                if (competitionTeam == null)
                {
                    AddTeamToHockeyCompetitionRequest addTeamReq = new AddTeamToHockeyCompetitionRequest { TeamId = team.Id };
                    HttpResponseMessage addTeamResp = await http.PostAsJsonAsync("api/HockeySeason/" + season.Id + "/teams", addTeamReq);
                    if (!addTeamResp.IsSuccessStatusCode)
                    {
                        // Idempotent caveat: duplicate add may fail; refresh and continue.
                        string body = await addTeamResp.Content.ReadAsStringAsync();
                        Console.WriteLine("Note: add team to season returned " + (int)addTeamResp.StatusCode + " for " + team.Name + ": " + Truncate(body));
                        season = await GetSeasonByIdAsync(http, jsonOptions, season.Id) ?? season;
                        competitionTeam = season.Teams.FirstOrDefault(t => t.TeamId == team.Id && t.IsActive);
                        if (competitionTeam == null)
                        {
                            await SeederHttp.EnsureSuccessWithBody(addTeamResp, "Add Hockey Team To Season");
                        }
                    }
                    else
                    {
                        ApiResponse<HockeyCompetitionTeamDto>? addApi =
                            await addTeamResp.Content.ReadFromJsonAsync<ApiResponse<HockeyCompetitionTeamDto>>(jsonOptions);
                        if (addApi?.Data == null)
                        {
                            throw new InvalidOperationException("Add hockey team to season returned empty payload.");
                        }

                        competitionTeam = addApi.Data;
                        Console.WriteLine("Added team " + team.Name + " to season " + season.Name);
                    }
                }

                if (competitionTeam == null)
                {
                    continue;
                }

                bool alreadyInDivision = competitionDivision.Teams.Any(t => t.CompetitionTeamId == competitionTeam.Id && t.IsActive);
                if (alreadyInDivision)
                {
                    Console.WriteLine("Team " + team.Name + " already in season division " + teamSeed.DivisionName + ", skipping");
                    continue;
                }

                AddTeamToHockeySeasonDivisionRequest placeReq = new AddTeamToHockeySeasonDivisionRequest
                {
                    CompetitionTeamId = competitionTeam.Id
                };
                HttpResponseMessage placeResp = await http.PostAsJsonAsync(
                    "api/HockeySeason/" + season.Id + "/divisions/" + competitionDivision.Id + "/teams",
                    placeReq);
                await SeederHttp.EnsureSuccessWithBody(placeResp, "Place Hockey Team In Season Division");
                Console.WriteLine("Placed team " + team.Name + " in season " + season.Name + " division " + teamSeed.DivisionName);

                season = await GetSeasonByIdAsync(http, jsonOptions, season.Id) ?? season;
            }
        }
    }

    public static async Task AddPlayersAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid teamId,
        List<HockeyTeamPlayerByEmailSeed> players,
        Dictionary<string, Guid> emailToPlayerId)
    {
        HashSet<int> existingJerseyNumbers = new HashSet<int>();
        HashSet<Guid> existingPlayerIds = new HashSet<Guid>();

        HttpResponseMessage teamResp = await http.GetAsync("api/HockeyTeam/" + teamId);
        if (teamResp.IsSuccessStatusCode)
        {
            ApiResponse<HockeyTeamDto>? teamApi = await teamResp.Content.ReadFromJsonAsync<ApiResponse<HockeyTeamDto>>(jsonOptions);
            if (teamApi?.Data?.Roster != null)
            {
                foreach (HockeyTeamPlayerDto rosterPlayer in teamApi.Data.Roster)
                {
                    if (rosterPlayer.JerseyNumber.HasValue)
                    {
                        existingJerseyNumbers.Add(rosterPlayer.JerseyNumber.Value);
                    }
                    existingPlayerIds.Add(rosterPlayer.PlayerId);
                }
            }
        }

        foreach (HockeyTeamPlayerByEmailSeed player in players)
        {
            if (!emailToPlayerId.TryGetValue(player.PersonEmail, out Guid playerId))
            {
                playerId = await ResolveOrCreatePlayerIdAsync(http, jsonOptions, player, emailToPlayerId);
            }

            if (existingPlayerIds.Contains(playerId))
            {
                continue;
            }

            if (existingJerseyNumbers.Contains(player.JerseyNumber))
            {
                Console.WriteLine("Jersey number already in use (" + player.JerseyNumber + ") for hockey team " + teamId + ", skipping " + player.PersonEmail);
                continue;
            }

            AddPlayerToHockeyTeamRequest request = new AddPlayerToHockeyTeamRequest
            {
                PlayerId = playerId,
                Position = player.Position,
                JerseyNumber = player.JerseyNumber,
                RosterStatus = HockeyRosterStatus.Active
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyTeam/" + teamId + "/players", request);
            await SeederHttp.EnsureSuccessWithBody(response, "Add Player To Hockey Team");

            existingJerseyNumbers.Add(player.JerseyNumber);
            existingPlayerIds.Add(playerId);
            Console.WriteLine("Added hockey player " + player.PersonEmail + " (#" + player.JerseyNumber + ") to team " + teamId);
        }
    }

    /// <summary>
    /// Seeds 1–2 team lines and optional head coach staff (idempotent by line name / PersonId).
    /// </summary>
    public static async Task SeedLinesAndStaffAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<HockeyTeamSeed> teamSeeds,
        List<HockeyTeamDto> teams,
        Dictionary<string, Guid> emailToPersonId)
    {
        foreach (HockeyTeamSeed teamSeed in teamSeeds)
        {
            HockeyTeamDto? listed = teams.FirstOrDefault(t =>
                string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
            if (listed == null)
            {
                continue;
            }

            HockeyTeamDto? team = await GetTeamByIdAsync(http, jsonOptions, listed.Id) ?? listed;
            await EnsureLinesAsync(http, jsonOptions, team);
            await EnsureStaffAsync(http, jsonOptions, team, teamSeed, emailToPersonId);
        }
    }

    private static async Task EnsureLinesAsync(HttpClient http, JsonSerializerOptions jsonOptions, HockeyTeamDto team)
    {
        List<HockeyTeamPlayerDto> active = team.Roster.Where(p => p.IsActive).ToList();
        List<HockeyTeamPlayerDto> forwards = active
            .Where(p => IsForward(p.Position))
            .Take(3)
            .ToList();
        List<HockeyTeamPlayerDto> defense = active
            .Where(p => string.Equals(p.Position, "Defenseman", StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToList();

        if (forwards.Count >= 3 && !team.Lines.Any(l => string.Equals(l.Name, "Line 1", StringComparison.OrdinalIgnoreCase)))
        {
            HockeyLineDto? line1 = await CreateLineAsync(http, jsonOptions, team.Id, "Line 1", 1, HockeyLineType.ForwardLine);
            if (line1 != null)
            {
                await AddLinePlayerAsync(http, team.Id, line1.Id, forwards[0].Id, HockeyLineSlot.Center, 0);
                await AddLinePlayerAsync(http, team.Id, line1.Id, forwards[1].Id, HockeyLineSlot.LeftWing, 1);
                await AddLinePlayerAsync(http, team.Id, line1.Id, forwards[2].Id, HockeyLineSlot.RightWing, 2);
                Console.WriteLine("Created Line 1 for " + team.Name);
            }
        }
        else if (team.Lines.Any(l => string.Equals(l.Name, "Line 1", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Line 1 exists on " + team.Name + ", skipping");
        }

        if (defense.Count >= 2 && !team.Lines.Any(l => string.Equals(l.Name, "Pair 1", StringComparison.OrdinalIgnoreCase)))
        {
            HockeyLineDto? pair = await CreateLineAsync(http, jsonOptions, team.Id, "Pair 1", 2, HockeyLineType.DefensePair);
            if (pair != null)
            {
                await AddLinePlayerAsync(http, team.Id, pair.Id, defense[0].Id, HockeyLineSlot.LeftDefense, 0);
                await AddLinePlayerAsync(http, team.Id, pair.Id, defense[1].Id, HockeyLineSlot.RightDefense, 1);
                Console.WriteLine("Created Pair 1 for " + team.Name);
            }
        }
        else if (team.Lines.Any(l => string.Equals(l.Name, "Pair 1", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Pair 1 exists on " + team.Name + ", skipping");
        }
    }

    private static async Task EnsureStaffAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyTeamDto team,
        HockeyTeamSeed teamSeed,
        Dictionary<string, Guid> emailToPersonId)
    {
        if (string.IsNullOrWhiteSpace(teamSeed.StaffPersonEmail))
        {
            return;
        }

        if (!emailToPersonId.TryGetValue(teamSeed.StaffPersonEmail, out Guid personId))
        {
            HttpResponseMessage personResp = await http.GetAsync(
                "api/persons/by-email?email=" + Uri.EscapeDataString(teamSeed.StaffPersonEmail));
            if (!personResp.IsSuccessStatusCode)
            {
                Console.WriteLine("Warning: staff person not found for " + teamSeed.StaffPersonEmail);
                return;
            }

            ApiResponse<PersonDto>? personApi = await personResp.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(jsonOptions);
            if (personApi?.Data == null)
            {
                return;
            }

            personId = personApi.Data.Id;
            emailToPersonId[teamSeed.StaffPersonEmail] = personId;
        }

        if (team.StaffMembers.Any(s => s.PersonId == personId && s.IsActive))
        {
            Console.WriteLine("Staff already on " + team.Name + " for person " + personId + ", skipping");
            return;
        }

        AddHockeyTeamStaffRequest request = new AddHockeyTeamStaffRequest
        {
            PersonId = personId,
            Role = HockeyTeamStaffRole.HeadCoach
        };
        HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyTeam/" + team.Id + "/staff", request);
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Warning: add staff failed for " + team.Name + ": " + Truncate(body));
            return;
        }

        Console.WriteLine("Added HeadCoach staff to " + team.Name);
    }

    private static async Task<HockeyLineDto?> CreateLineAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid teamId,
        string name,
        int lineNumber,
        HockeyLineType lineType)
    {
        AddHockeyLineRequest request = new AddHockeyLineRequest
        {
            Name = name,
            LineNumber = lineNumber,
            LineType = lineType
        };
        HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeyTeam/" + teamId + "/lines", request);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Warning: create line failed: " + await response.Content.ReadAsStringAsync());
            return null;
        }

        ApiResponse<HockeyTeamDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeyTeamDto>>(jsonOptions);
        return api?.Data?.Lines.FirstOrDefault(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task AddLinePlayerAsync(
        HttpClient http,
        Guid teamId,
        Guid lineId,
        Guid teamPlayerId,
        HockeyLineSlot slot,
        int order)
    {
        AddPlayerToHockeyLineRequest request = new AddPlayerToHockeyLineRequest
        {
            TeamPlayerId = teamPlayerId,
            Slot = slot,
            Order = order
        };
        HttpResponseMessage response = await http.PostAsJsonAsync(
            "api/HockeyTeam/" + teamId + "/lines/" + lineId + "/players",
            request);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Warning: add line player failed: " + await response.Content.ReadAsStringAsync());
        }
    }

    private static async Task<HockeyTeamDto?> GetTeamByIdAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid teamId)
    {
        HttpResponseMessage resp = await http.GetAsync("api/HockeyTeam/" + teamId);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<HockeyTeamDto>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<HockeyTeamDto>>(jsonOptions);
        return api?.Data;
    }

    private static bool IsForward(string position) =>
        string.Equals(position, "Center", StringComparison.OrdinalIgnoreCase)
        || string.Equals(position, "LeftWing", StringComparison.OrdinalIgnoreCase)
        || string.Equals(position, "RightWing", StringComparison.OrdinalIgnoreCase);

    private static async Task<Guid> ResolveOrCreatePlayerIdAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyTeamPlayerByEmailSeed player,
        Dictionary<string, Guid> emailToPlayerId)
    {
        HttpResponseMessage personResp = await http.GetAsync("api/persons/by-email?email=" + Uri.EscapeDataString(player.PersonEmail));
        if (!personResp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("No person found for email: " + player.PersonEmail);
        }

        ApiResponse<PersonDto>? personApi = await personResp.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(jsonOptions);
        if (personApi?.Data == null)
        {
            throw new InvalidOperationException("Failed to fetch person for email: " + player.PersonEmail);
        }

        Guid personId = personApi.Data.Id;

        CreateHockeyPlayerRequest createReq = new CreateHockeyPlayerRequest
        {
            PersonId = personId,
            PrimaryPosition = player.Position,
            Shoots = HockeyShoots.Unknown,
            Catches = player.Position == HockeyPosition.Goalie ? HockeyCatches.Unknown : null
        };

        HttpResponseMessage createResp = await http.PostAsJsonAsync("api/HockeyPlayer", createReq);
        if (createResp.IsSuccessStatusCode)
        {
            ApiResponse<HockeyPlayerDto>? createApi = await createResp.Content.ReadFromJsonAsync<ApiResponse<HockeyPlayerDto>>(jsonOptions);
            if (createApi?.Data == null)
            {
                throw new InvalidOperationException("Create hockey player (fallback) failed for email: " + player.PersonEmail);
            }

            emailToPlayerId[player.PersonEmail] = createApi.Data.Id;
            return createApi.Data.Id;
        }

        // Player may already exist; scan teams for PersonId.
        Dictionary<Guid, HockeyPlayerDto> byPerson = await HockeyPlayersSeeder.LoadExistingPlayersByPersonIdAsync(http, jsonOptions);
        if (byPerson.TryGetValue(personId, out HockeyPlayerDto? existing))
        {
            emailToPlayerId[player.PersonEmail] = existing.Id;
            return existing.Id;
        }

        await SeederHttp.EnsureSuccessWithBody(createResp, "Create Hockey Player (fallback)");
        return Guid.Empty;
    }

    private static async Task<HockeySeasonDto?> GetSeasonByIdAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid seasonId)
    {
        HttpResponseMessage resp = await http.GetAsync("api/HockeySeason/" + seasonId);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<HockeySeasonDto>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<HockeySeasonDto>>(jsonOptions);
        return api?.Data;
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

    private static string Truncate(string body) => body.Length > 300 ? body.Substring(0, 300) + "..." : body;
}
