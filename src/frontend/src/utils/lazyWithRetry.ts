import { lazy, type ComponentType } from 'react';

/**
 * Wraps React.lazy() with retry logic for dynamic imports.
 * When a chunk fails to load (e.g. after a new deployment changes file hashes),
 * the page is automatically reloaded once to fetch the updated manifest.
 * A sessionStorage flag prevents infinite reload loops.
 */
// Using ComponentType<any> here so the generic preserves the component's
// own prop signature (so callers can pass component-specific props like `mode`).
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function lazyWithRetry<T extends ComponentType<any>>(
  importFn: () => Promise<{ default: T }>,
) {
  return lazy(() =>
    importFn().catch((error: unknown) => {
      const isChunkError =
        error instanceof TypeError &&
        (error.message.includes('Failed to fetch dynamically imported module') ||
          error.message.includes('error loading dynamically imported module') ||
          error.message.includes('Importing a module script failed'));

      if (!isChunkError) throw error;

      const reloadKey = 'chunk-reload-retry';
      const hasReloaded = sessionStorage.getItem(reloadKey);

      if (!hasReloaded) {
        sessionStorage.setItem(reloadKey, '1');
        window.location.reload();
        return new Promise<never>(() => {});
      }

      sessionStorage.removeItem(reloadKey);
      throw error;
    }),
  );
}
