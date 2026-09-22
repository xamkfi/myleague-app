import { unwrapApiErrorMessage } from './ParseErrorResponse';

const NOT_FOUND_PATTERNS = [
  '404',
  'was not found',
  'season statistics with key',
  'no statistics found',
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

  const raw =
    error instanceof Error
      ? error.message
      : typeof error === 'string'
        ? error
        : String(error ?? '');
  const unwrapped = unwrapApiErrorMessage(error, raw);
  const haystacks = [raw, unwrapped].map((value) => value.toLowerCase());
  return haystacks.some((text) => NOT_FOUND_PATTERNS.some((pattern) => text.includes(pattern)));
}
