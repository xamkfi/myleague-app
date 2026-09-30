import type { TFunction } from 'i18next';

/**
 * `fetch` rejects with a bare `TypeError` when the request never reached the server
 * (offline, DNS, CORS, or the browser refusing new connections). These messages differ per
 * browser, so match loosely.
 */
export function isNetworkError(error: unknown): boolean {
  if (!(error instanceof TypeError)) return false;
  const message: string = error.message.toLowerCase();
  return message.includes('failed to fetch')
    || message.includes('networkerror')
    || message.includes('load failed')
    || message.includes('network request failed');
}

/**
 * Turns an unknown thrown value into a user-facing message. Network failures get a
 * translated explanation instead of the browser's raw `Failed to fetch`.
 */
export function describeMatchError(error: unknown, fallback: string, t: TFunction): string {
  if (isNetworkError(error)) {
    return t(
      'floorball.matches.manage.errors.connection',
      'Could not reach the server. Check your connection and try again.',
    );
  }
  if (error instanceof Error && error.message) {
    return error.message;
  }
  return fallback;
}
