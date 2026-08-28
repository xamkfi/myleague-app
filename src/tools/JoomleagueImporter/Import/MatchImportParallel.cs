namespace JoomleagueImporter.Import;

/// <summary>
/// Bounded parallelism for match imports and whole-season imports.
/// Seasons use separate competition ids, so they do not contend on the same
/// season-stat unique keys the way two matches in one season do.
/// </summary>
internal static class MatchImportParallel
{
    public static int Degree { get; set; } = 4;

    public static int SeasonDegree { get; set; } = 2;

    public static Task ForEachAsync<T>(IReadOnlyList<T> items, Func<T, Task> body) =>
        ForEachAsync(items, Degree, body);

    public static Task ForEachSeasonAsync<T>(IReadOnlyList<T> items, Func<T, Task> body) =>
        ForEachAsync(items, SeasonDegree, body);

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
