/** Keep parallel API calls small enough that they do not exhaust the database connection limit. */
const DEFAULT_CONCURRENCY = 4;

export async function mapWithConcurrency<T, R>(
  items: readonly T[],
  mapper: (item: T) => Promise<R>,
  concurrency: number = DEFAULT_CONCURRENCY,
): Promise<R[]> {
  if (items.length === 0) {
    return [];
  }

  const results = new Array<R>(items.length);
  let nextIndex = 0;
  const workerCount = Math.min(Math.max(concurrency, 1), items.length);

  async function worker(): Promise<void> {
    while (nextIndex < items.length) {
      const index = nextIndex;
      nextIndex += 1;
      results[index] = await mapper(items[index]);
    }
  }

  await Promise.all(Array.from({ length: workerCount }, () => worker()));
  return results;
}
