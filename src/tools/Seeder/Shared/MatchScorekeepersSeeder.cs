using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Shared.DTOs;
using WebAPI.Models.Common;

namespace Seeder;

/// <summary>
/// Assigns optional scorekeepers (toimitsijat) to seeded matches through
/// <c>POST {matchesPath}/{matchId}/scorekeepers</c>. Idempotent: persons already on the match are skipped.
/// </summary>
public static class MatchScorekeepersSeeder
{
    public static async Task<Dictionary<string, Guid>> SeedPersonsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<PersonSeed> scorekeeperPersons)
    {
        if (scorekeeperPersons.Count == 0)
        {
            return new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        }

        (_, Dictionary<string, Guid> emailToPersonId) =
            await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, scorekeeperPersons);
        return emailToPersonId;
    }

    public static async Task EnsureAsync(
        HttpClient http,
        string matchesPath,
        Guid matchId,
        IReadOnlyCollection<MatchPersonDto>? existingScorekeepers,
        IReadOnlyCollection<string> scorekeeperEmails,
        IReadOnlyDictionary<string, Guid> emailToPersonId)
    {
        if (scorekeeperEmails.Count == 0)
        {
            return;
        }

        HashSet<Guid> existingIds = (existingScorekeepers ?? Array.Empty<MatchPersonDto>())
            .Select(s => s.Id)
            .ToHashSet();

        foreach (string email in scorekeeperEmails)
        {
            if (!emailToPersonId.TryGetValue(email, out Guid personId))
            {
                Console.WriteLine($"Warning: scorekeeper person not found for email: {email}");
                continue;
            }

            if (!existingIds.Add(personId))
            {
                continue;
            }

            AddMatchScorekeeperRequest request = new AddMatchScorekeeperRequest { PersonId = personId };
            try
            {
                HttpResponseMessage response = await http.PostAsJsonAsync($"{matchesPath}/{matchId}/scorekeepers", request);
                await SeederHttp.EnsureSuccessWithBody(response, "Add Match Scorekeeper");
                Console.WriteLine($"Added scorekeeper {email} to match {matchId}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Warning: failed to add scorekeeper {email} to match {matchId}: {ex.Message}");
            }
        }
    }
}
