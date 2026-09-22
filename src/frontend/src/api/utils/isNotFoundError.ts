import { unwrapApiErrorMessage } from './ParseErrorResponse';

const NOT_FOUND_PATTERNS = [
  'was not found',
  'season statistics with key',
  'no statistics found',
  'not found',
];

export function isNotFoundError(err: unknown): boolean {
  const raw = err instanceof Error ? err.message : String(err ?? '');
  const unwrapped = unwrapApiErrorMessage(err, raw);
  const haystacks = [raw, unwrapped].map((value) => value.toLowerCase());
  return haystacks.some((text) => NOT_FOUND_PATTERNS.some((pattern) => text.includes(pattern)));
}
