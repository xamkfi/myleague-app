import type { AuthUserRole } from '../types/auth/authTypes';

/** Landing page of the area each signed-in role works in. */
export function getRoleHome(role: AuthUserRole): string {
  switch (role) {
    case 'ClubAdmin':
      return '/club-admin';
    case 'MatchOfficial':
      return '/official';
    case 'SystemAdmin':
    default:
      return '/admin';
  }
}
