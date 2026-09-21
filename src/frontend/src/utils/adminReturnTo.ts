const PREV_KEY = 'myleague_admin_previous_path';
const CUR_KEY = 'myleague_admin_current_path';

const UUID_RE =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

const ADMIN_LIST_ROOTS = new Set([
  '/admin',
  '/admin/clubs',
  '/admin/persons',
  '/admin/divisions',
  '/admin/news',
  '/admin/users',
  '/admin/settings',
  '/admin/site-content',
  '/admin/site-content/info-pages',
  '/admin/site-content/rules',
  '/admin/site-content/footer-contacts',
  '/admin/floorball',
  '/admin/floorball/teams',
  '/admin/floorball/players',
  '/admin/floorball/seasons',
  '/admin/floorball/tournaments',
  '/admin/floorball/matches',
  '/admin/floorball/referees',
  '/admin/football',
  '/admin/football/teams',
  '/admin/football/players',
  '/admin/football/seasons',
  '/admin/football/tournaments',
  '/admin/football/matches',
  '/admin/football/referees',
  '/admin/hockey',
  '/admin/hockey/teams',
  '/admin/hockey/players',
  '/admin/hockey/seasons',
  '/admin/hockey/tournaments',
  '/admin/hockey/matches',
  '/admin/hockey/officials',
]);

export function sanitizeAdminReturnPath(raw: string | null | undefined): string | null {
  if (!raw) {
    return null;
  }

  let decoded = raw;
  try {
    decoded = decodeURIComponent(raw);
  } catch {
    return null;
  }

  if (!decoded.startsWith('/admin')) {
    return null;
  }
  if (decoded.startsWith('//') || decoded.includes('://')) {
    return null;
  }

  return decoded;
}

export function isAdminListRoot(pathname: string): boolean {
  const normalized = pathname.replace(/\/$/, '') || '/admin';
  return ADMIN_LIST_ROOTS.has(normalized);
}

export function inferAdminParentPath(pathname: string): string {
  const segments = pathname.replace(/\/$/, '').split('/').filter(Boolean);
  if (segments.length <= 1) {
    return '/admin';
  }

  const last = segments[segments.length - 1] ?? '';
  const previous = segments[segments.length - 2] ?? '';

  if (last === 'edit' && UUID_RE.test(previous)) {
    segments.pop();
    segments.pop();
  } else if (UUID_RE.test(last) && (previous === 'edit' || previous === 'manage')) {
    segments.pop();
    segments.pop();
  } else {
    segments.pop();
  }

  const parent = `/${segments.join('/')}`;
  return parent.startsWith('/admin') ? parent : '/admin';
}

export function createAdminFromState(pathname: string, search = ''): { from: string } {
  return { from: `${pathname}${search}` };
}

export function rememberAdminLocation(path: string): void {
  if (!path.startsWith('/admin')) {
    return;
  }

  try {
    const storedCurrent = sessionStorage.getItem(CUR_KEY);
    if (storedCurrent && storedCurrent !== path && storedCurrent.startsWith('/admin')) {
      sessionStorage.setItem(PREV_KEY, storedCurrent);
    }
    sessionStorage.setItem(CUR_KEY, path);
  } catch {
    // sessionStorage may be unavailable
  }
}

export function readRememberedAdminReturnPath(currentPath: string): string | null {
  try {
    const storedCurrent = sanitizeAdminReturnPath(sessionStorage.getItem(CUR_KEY));
    const storedPrevious = sanitizeAdminReturnPath(sessionStorage.getItem(PREV_KEY));
    if (storedCurrent && storedCurrent !== currentPath && !storedCurrent.startsWith(currentPath + '?')) {
      return storedCurrent;
    }
    if (storedPrevious && storedPrevious !== currentPath) {
      return storedPrevious;
    }
    return null;
  } catch {
    return null;
  }
}

export function resolveAdminReturnTo(
  pathname: string,
  search: string,
  stateFrom: string | null | undefined,
  queryReturnTo: string | null | undefined,
): string {
  const current = `${pathname}${search}`;
  const candidates = [
    sanitizeAdminReturnPath(stateFrom),
    sanitizeAdminReturnPath(queryReturnTo),
    readRememberedAdminReturnPath(current),
  ];

  for (const candidate of candidates) {
    if (candidate && candidate !== current && candidate !== pathname) {
      return candidate;
    }
  }

  return inferAdminParentPath(pathname);
}
