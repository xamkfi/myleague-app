namespace JoomleagueImporter.Import;

/// <summary>
/// Runs independent match imports with a bounded degree of parallelism.
/// </summary>
internal static class MatchImportParallel
{
    public static int Degree { get; set; } = 4;

    public static async Task ForEachAsync<T>(IReadOnlyList<T> items, Func<T, Task> body)
    {
        if (items.Count == 0)
            return;

        int degree = Math.Max(1, Degree);
        if (degree == 1)
        {
            foreach (T item in items)
                await body(item);
            return;
        }

        await Parallel.ForEachAsync(
            items,
            new ParallelOptions { MaxDegreeOfParallelism = degree },
            async (item, _) => await body(item));
    }
}
