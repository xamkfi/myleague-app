namespace JoomleagueImporter.Import;

/// <summary>
/// Bounded parallelism for club, person, team, match, and whole-season imports.
/// Seasons use separate competition ids, so they do not contend on the same
/// season-stat unique keys the way two matches in one season do.
/// </summary>
internal static class MatchImportParallel
{
    public static int Degree { get; set; } = 4;

    public static int SeasonDegree { get; set; } = 2;

    public static int PersonDegree { get; set; } = 8;

    public static int ClubDegree { get; set; } = 8;

    public static int TeamDegree { get; set; } = 8;

    public static Task ForEachAsync<T>(IReadOnlyList<T> items, Func<T, Task> body) =>
        ForEachAsync(items, Degree, body);

    public static Task ForEachSeasonAsync<T>(IReadOnlyList<T> items, Func<T, Task> body) =>
        ForEachAsync(items, SeasonDegree, body);

    public static Task ForEachPersonAsync<T>(IReadOnlyList<T> items, Func<T, Task> body) =>
        ForEachAsync(items, PersonDegree, body);

    public static Task ForEachClubAsync<T>(IReadOnlyList<T> items, Func<T, Task> body) =>
        ForEachAsync(items, ClubDegree, body);

    public static Task ForEachTeamAsync<T>(IReadOnlyList<T> items, Func<T, Task> body) =>
        ForEachAsync(items, TeamDegree, body);

    public static async Task ForEachAsync<T>(IReadOnlyList<T> items, int degree, Func<T, Task> body)
    {
        if (items.Count == 0)
            return;

        int bounded = Math.Max(1, degree);
        if (bounded == 1)
        {
            foreach (T item in items)
                await body(item);
            return;
        }

        await Parallel.ForEachAsync(
            items,
            new ParallelOptions { MaxDegreeOfParallelism = bounded },
            async (item, _) => await body(item));
    }
}
