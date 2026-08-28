using System.Collections.Concurrent;
using Application.Features.Common.Clubs.DTOs;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Shared club import for all sports. One work item per JoomLeague club key so
/// two teams in the same club do not both create it.
/// </summary>
internal static class ClubEntityImport
{
    public static async Task ImportAsync(
        ImportApiClient api,
        IdMapStore idMap,
        ImportLogger log,
        FloorballImportSet set,
        JoomleagueDatabase db)
    {
        Console.WriteLine("--- Clubs ---");

        List<(int Key, string Name, string City)> pending = [];
        HashSet<int> seen = [];
        foreach (OldTeam team in set.UniqueTeams.Values)
        {
            OldClub? oldClub = team.ClubId.HasValue ? db.Clubs.GetValueOrDefault(team.ClubId.Value) : null;
            int oldClubKey = oldClub?.Id ?? -team.Id;
            if (!seen.Add(oldClubKey) || idMap.HasClub(oldClubKey))
                continue;

            string clubName = !string.IsNullOrWhiteSpace(oldClub?.Name) ? oldClub!.Name : team.Name;
            string city = !string.IsNullOrWhiteSpace(oldClub?.Location) ? oldClub!.Location : "Mikkeli";
            pending.Add((oldClubKey, clubName, city));
        }

        ConcurrentDictionary<string, ClubDto> byName = new(StringComparer.OrdinalIgnoreCase);
        foreach (ClubDto club in await api.GetClubsAsync())
            byName.TryAdd(club.Name, club);

        int created = 0, reused = 0, failed = 0;
        Console.WriteLine($"  Importing {pending.Count} clubs (concurrency {MatchImportParallel.ClubDegree})...");
        await MatchImportParallel.ForEachClubAsync(pending, async item =>
        {
            if (byName.TryGetValue(item.Name, out ClubDto? club))
            {
                Interlocked.Increment(ref reused);
                idMap.MapClub(item.Key, club.Id);
                return;
            }

            club = await api.CreateClubAsync(item.Name, item.City);
            if (club == null)
            {
                foreach (ClubDto refreshed in await api.GetClubsAsync())
                    byName.TryAdd(refreshed.Name, refreshed);
                byName.TryGetValue(item.Name, out club);
            }

            if (club == null)
            {
                log.LogError("CreateClub", new { item.Name }, "API returned null.");
                Interlocked.Increment(ref failed);
                return;
            }

            byName.TryAdd(club.Name, club);
            Interlocked.Increment(ref created);
            idMap.MapClub(item.Key, club.Id);
        });

        idMap.Save(force: true);
        Console.WriteLine($"  Clubs: {created} created, {reused} already existed, {failed} failed.");
    }
}
