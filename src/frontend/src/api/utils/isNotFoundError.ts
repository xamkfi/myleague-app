const NOT_FOUND_PATTERNS = [
  '404',
  'not found',
  'notfound',
  'ei löytynyt',
  'could not find',
];

export function isNotFoundError(error: unknown): boolean {
  if (error && typeof error === 'object' && 'status' in error) {
    const status = (error as { status?: number }).status;
    if (status === 404) {
      return true;
    }
  }

  const message =
    error instanceof Error
      ? error.message
      : typeof error === 'string'
        ? error
        : '';

  if (!message) {
    return false;
  }

  const lower = message.toLowerCase();
  return NOT_FOUND_PATTERNS.some((pattern) => lower.includes(pattern));
}
