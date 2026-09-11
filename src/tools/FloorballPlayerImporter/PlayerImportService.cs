using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using FloorballPlayerImporter.Models;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Floorball;

namespace FloorballPlayerImporter;

/// <summary>
/// Service for importing floorball players from JSON files
/// </summary>
public class PlayerImportService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public PlayerImportService(HttpClient httpClient, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// Import players from JSON files in the specified directory
    /// </summary>
    public async Task<ImportStatistics> ImportFromJsonFilesAsync(string dataFilesPath)
    {
        ImportStatistics stats = new ImportStatistics();

        string[] jsonFiles = Directory.GetFiles(dataFilesPath, "*.json");

        if (jsonFiles.Length == 0)
        {
            Console.WriteLine("No JSON files found in DataFiles folder.");
            return stats;
        }

        Console.WriteLine($"Found {jsonFiles.Length} JSON file(s) to process.\n");

        foreach (string jsonFile in jsonFiles)
        {
            Console.WriteLine($"Processing file: {Path.GetFileName(jsonFile)}");
            try
            {
                await ProcessJsonFileAsync(jsonFile, stats);
            }
            catch (Exception ex)
            {
                string error = $"Failed to process file {Path.GetFileName(jsonFile)}: {ex.Message}";
                stats.Errors.Add(error);
                stats.Failed++;
                Console.Error.WriteLine($"  ERROR: {error}");
            }
            Console.WriteLine();
        }

        return stats;
    }

    /// <summary>
    /// Process a single JSON file
    /// </summary>
    private async Task ProcessJsonFileAsync(string jsonFile, ImportStatistics stats)
    {
        // Read and parse JSON
        string jsonContent = await File.ReadAllTextAsync(jsonFile);
        TeamRosterImport? roster = JsonSerializer.Deserialize<TeamRosterImport>(jsonContent, _jsonOptions);

        if (roster == null || string.IsNullOrWhiteSpace(roster.Team))
        {
            stats.Errors.Add($"Invalid JSON format in file {Path.GetFileName(jsonFile)}");
            stats.Failed++;
            return;
        }

        Console.WriteLine($"  Team: {roster.Team}");
        Console.WriteLine($"  Players in file: {roster.Players.Count}");

        // Find or create the team by name
        FloorballTeamDto? team = await FindOrCreateTeamAsync(roster.Team, stats);
        if (team == null)
        {
            string error = $"Failed to find or create team: {roster.Team}";
            stats.Errors.Add(error);
            stats.Failed++;
            Console.Error.WriteLine($"  ERROR: {error}");
            return;
        }

        Console.WriteLine($"  Using team (ID: {team.Id})");

        // Get existing roster to check for duplicate jersey numbers
        HashSet<int> existingJerseyNumbers = await GetExistingJerseyNumbersAsync(team.Id);
        Console.WriteLine($"  Existing jersey numbers on team: {existingJerseyNumbers.Count}");

        // Process each player
        foreach (PlayerImport playerImport in roster.Players)
        {
            stats.TotalProcessed++;
            await ProcessPlayerAsync(playerImport, team.Id, existingJerseyNumbers, stats);
        }
    }

    /// <summary>
    /// Process a single player import
    /// </summary>
    private async Task ProcessPlayerAsync(
        PlayerImport playerImport,
        Guid teamId,
        HashSet<int> existingJerseyNumbers,
        ImportStatistics stats)
    {
        try
        {
            Console.WriteLine($"  Processing: {playerImport.FullName} (#{playerImport.JerseyNumber}, {playerImport.Position})");

            // Check for duplicate jersey number (allow 0)
            if (playerImport.JerseyNumber != 0 && existingJerseyNumbers.Contains(playerImport.JerseyNumber))
            {
                stats.PlayersSkippedDuplicateJersey++;
                stats.PlayersDuplicateJersey.Add((playerImport.FullName, playerImport.JerseyNumber));
                Console.WriteLine($"    SKIPPED: Jersey number {playerImport.JerseyNumber} already in use");
                return;
            }

            // Search for person by name
            PersonDto? person = await FindPersonByNameAsync(playerImport.FirstName, playerImport.LastName);
            if (person == null)
            {
                stats.PlayersSkippedPersonNotFound++;
                stats.PlayersPersonNotFound.Add(playerImport.FullName);
                Console.WriteLine($"    SKIPPED: Person not found");
                return;
            }

            Console.WriteLine($"    Found person (ID: {person.Id})");

            // Check if FloorballPlayer exists for this person
            FloorballPlayerDto? player = await FindOrCreatePlayerAsync(person.Id, stats);
            if (player == null)
            {
                stats.Failed++;
                stats.FailedPlayers.Add((playerImport.FullName, "Failed to create FloorballPlayer"));
                Console.Error.WriteLine($"    ERROR: Failed to create FloorballPlayer");
                return;
            }

            // Parse position
            FloorballPosition position = ParsePosition(playerImport.Position);

            // Add player to team
            bool success = await AddPlayerToTeamAsync(teamId, player.Id, position, playerImport.JerseyNumber);
            if (success)
            {
                stats.PlayersAssignedToTeam++;
                stats.PlayersAssigned.Add($"{playerImport.FullName} (#{playerImport.JerseyNumber})");
                existingJerseyNumbers.Add(playerImport.JerseyNumber);
                Console.WriteLine($"    SUCCESS: Added to team as {position} with jersey #{playerImport.JerseyNumber}");
            }
            else
            {
                stats.Failed++;
                stats.FailedPlayers.Add((playerImport.FullName, "Failed to add to team"));
                Console.Error.WriteLine($"    ERROR: Failed to add player to team");
            }
        }
        catch (Exception ex)
        {
            stats.Failed++;
            stats.FailedPlayers.Add((playerImport.FullName, ex.Message));
            Console.Error.WriteLine($"    ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Find a team by name (case-insensitive)
    /// </summary>
    private async Task<FloorballTeamDto?> FindTeamByNameAsync(string teamName)
    {
        HttpResponseMessage response = await _httpClient.GetAsync("api/floorballteam?Page=1&PageSize=0");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        PaginatedApiResponse<FloorballTeamDto>? apiResponse = 
            await response.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballTeamDto>>(_jsonOptions);

        if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
        {
            return null;
        }

        return apiResponse.Data.FirstOrDefault(t => 
            string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find a club by name (case-insensitive)
    /// </summary>
    private async Task<ClubDto?> FindClubByNameAsync(string clubName)
    {
        HttpResponseMessage response = await _httpClient.GetAsync("api/clubs");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<List<ClubDto>>? apiResponse = 
            await response.Content.ReadFromJsonAsync<ApiResponse<List<ClubDto>>>(_jsonOptions);

        if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
        {
            return null;
        }

        return apiResponse.Data.FirstOrDefault(c => 
            string.Equals(c.Name, clubName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find or create a club by name
    /// </summary>
    private async Task<ClubDto?> FindOrCreateClubAsync(string clubName, ImportStatistics stats)
    {
        // Try to find existing club
        ClubDto? existingClub = await FindClubByNameAsync(clubName);
        if (existingClub != null)
        {
            Console.WriteLine($"  Found existing club: {existingClub.Name} (ID: {existingClub.Id})");
            return existingClub;
        }

        // Create new club
        Console.WriteLine($"  Creating new club: {clubName}");
        CreateClubRequest clubRequest = new CreateClubRequest
        {
            Name = clubName,
            City = null,
            Country = null,
            FoundingDate = null,
            WebsiteUrl = null,
            LogoUrl = null,
            ContactEmail = null
        };

        HttpResponseMessage createResponse = await _httpClient.PostAsJsonAsync("api/clubs", clubRequest);
        if (!createResponse.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"  ERROR: Failed to create club. Status: {createResponse.StatusCode}");
            return null;
        }

        ApiResponse<ClubDto>? createApiResponse = 
            await createResponse.Content.ReadFromJsonAsync<ApiResponse<ClubDto>>(_jsonOptions);

        if (createApiResponse != null && createApiResponse.Success && createApiResponse.Data != null)
        {
            stats.ClubsCreated++;
            Console.WriteLine($"  Created new club: {createApiResponse.Data.Name} (ID: {createApiResponse.Data.Id})");
            return createApiResponse.Data;
        }

        return null;
    }

    /// <summary>
    /// Find or create a team by name
    /// </summary>
    private async Task<FloorballTeamDto?> FindOrCreateTeamAsync(string teamName, ImportStatistics stats)
    {
        // Try to find existing team
        FloorballTeamDto? existingTeam = await FindTeamByNameAsync(teamName);
        if (existingTeam != null)
        {
            Console.WriteLine($"  Found existing team: {existingTeam.Name} (ID: {existingTeam.Id})");
            return existingTeam;
        }

        // Team doesn't exist, so we need to create it
        // First, find or create the club (use same name as team)
        Console.WriteLine($"  Team not found, creating new team: {teamName}");
        ClubDto? club = await FindOrCreateClubAsync(teamName, stats);
        if (club == null)
        {
            Console.Error.WriteLine($"  ERROR: Failed to find or create club for team: {teamName}");
            return null;
        }

        // Create new team
        FloorballTeamRequest teamRequest = new FloorballTeamRequest
        {
            Name = teamName,
            ClubId = club.Id,
            DivisionId = null,
            HomeArena = "TBD",
            PrimaryJerseyColor = "White",
            SecondaryJerseyColor = "Black",
            Category = TeamCategory.Adult
        };

        HttpResponseMessage createResponse = await _httpClient.PostAsJsonAsync("api/floorballteam", teamRequest);
        if (!createResponse.IsSuccessStatusCode)
        {
            string errorContent = await createResponse.Content.ReadAsStringAsync();
            Console.Error.WriteLine($"  ERROR: Failed to create team. Status: {createResponse.StatusCode}");
            Console.Error.WriteLine($"  Response: {errorContent}");
            return null;
        }

        ApiResponse<FloorballTeamDto>? createApiResponse = 
            await createResponse.Content.ReadFromJsonAsync<ApiResponse<FloorballTeamDto>>(_jsonOptions);

        if (createApiResponse != null && createApiResponse.Success && createApiResponse.Data != null)
        {
            stats.TeamsCreated++;
            Console.WriteLine($"  Created new team: {createApiResponse.Data.Name} (ID: {createApiResponse.Data.Id})");
            return createApiResponse.Data;
        }

        return null;
    }

    /// <summary>
    /// Get existing jersey numbers for a team
    /// </summary>
    private async Task<HashSet<int>> GetExistingJerseyNumbersAsync(Guid teamId)
    {
        HashSet<int> jerseyNumbers = new HashSet<int>();

        HttpResponseMessage response = await _httpClient.GetAsync($"api/floorballteam/{teamId}");
        if (!response.IsSuccessStatusCode)
        {
            return jerseyNumbers;
        }

        ApiResponse<FloorballTeamDto>? apiResponse = 
            await response.Content.ReadFromJsonAsync<ApiResponse<FloorballTeamDto>>(_jsonOptions);

        if (apiResponse != null && apiResponse.Success && apiResponse.Data?.Roster != null)
        {
            foreach (FloorballTeamPlayerDto rosterPlayer in apiResponse.Data.Roster)
            {
                if (rosterPlayer.JerseyNumber.HasValue)
                {
                    jerseyNumbers.Add(rosterPlayer.JerseyNumber.Value);
                }
            }
        }

        return jerseyNumbers;
    }

    /// <summary>
    /// Find a person by first and last name
    /// </summary>
    private async Task<PersonDto?> FindPersonByNameAsync(string firstName, string lastName)
    {
        string fullName = $"{firstName} {lastName}".Trim();
        HttpResponseMessage response = await _httpClient.GetAsync(
            $"api/persons/search?name={Uri.EscapeDataString(fullName)}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<List<PersonDto>>? apiResponse = 
            await response.Content.ReadFromJsonAsync<ApiResponse<List<PersonDto>>>(_jsonOptions);

        if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
        {
            return null;
        }

        // Find exact match by first and last name
        return apiResponse.Data.FirstOrDefault(p =>
            string.Equals(p.FirstName, firstName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.LastName, lastName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find or create a FloorballPlayer for a person
    /// </summary>
    private async Task<FloorballPlayerDto?> FindOrCreatePlayerAsync(Guid personId, ImportStatistics stats)
    {
        // Check if player already exists
        HttpResponseMessage listResponse = await _httpClient.GetAsync("api/floorballplayer?Page=1&PageSize=0&IsActive=");
        if (listResponse.IsSuccessStatusCode)
        {
            PaginatedApiResponse<FloorballPlayerDto>? listApi = 
                await listResponse.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballPlayerDto>>(_jsonOptions);

            if (listApi != null && listApi.Success && listApi.Data != null)
            {
                FloorballPlayerDto? existing = listApi.Data.FirstOrDefault(p => p.PersonId == personId);
                if (existing != null)
                {
                    Console.WriteLine($"    Using existing FloorballPlayer (ID: {existing.Id})");
                    return existing;
                }
            }
        }

        // Create new player
        CreateFloorballPlayerRequest request = new CreateFloorballPlayerRequest
        {
            PersonId = personId
        };

        HttpResponseMessage createResponse = await _httpClient.PostAsJsonAsync("api/floorballplayer", request);
        if (!createResponse.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<FloorballPlayerDto>? createApi = 
            await createResponse.Content.ReadFromJsonAsync<ApiResponse<FloorballPlayerDto>>(_jsonOptions);

        if (createApi != null && createApi.Success && createApi.Data != null)
        {
            stats.PlayersCreated++;
            Console.WriteLine($"    Created new FloorballPlayer (ID: {createApi.Data.Id})");
            return createApi.Data;
        }

        return null;
    }

    /// <summary>
    /// Add a player to a team with position and jersey number
    /// </summary>
    private async Task<bool> AddPlayerToTeamAsync(
        Guid teamId,
        Guid playerId,
        FloorballPosition position,
        int jerseyNumber)
    {
        int positionValue = (int)position;
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"api/floorballteam/{teamId}/players/{playerId}?position={positionValue}&jerseyNumber={jerseyNumber}",
            null);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Parse position string to enum
    /// </summary>
    private FloorballPosition ParsePosition(string position)
    {
        return position.ToLowerInvariant() switch
        {
            "forward" => FloorballPosition.Forward,
            "center" => FloorballPosition.Center,
            "defender" => FloorballPosition.Defender,
            "goalie" => FloorballPosition.Goalkeeper,
            "goalkeeper" => FloorballPosition.Goalkeeper,
            _ => FloorballPosition.None
        };
    }
}

