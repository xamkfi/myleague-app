/**
 * Which area opened a sport's live match desk. `admin` is the full admin view; `official` is the
 * match official view, which hides match editing, team assignment, and officials/scorekeeper
 * assignment, and returns to `/official`.
 */
export type ManageMatchMode = 'admin' | 'official';
