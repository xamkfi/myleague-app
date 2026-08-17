export type ClubAdminSport = 'floorball' | 'football';

/** A team under a club managed by the current club admin. */
export interface ClubAdminTeam {
  sport: ClubAdminSport;
  teamId: string;
  name: string;
  shortName: string;
  logoUrl?: string | null;
}

/** A club managed by the current club admin, as returned by GET /api/club-admin/my-clubs. */
export interface ClubAdminClub {
  clubId: string;
  name: string;
  city: string;
  logoUrl?: string | null;
  teams: ClubAdminTeam[];
}

/** A user administering a club, as returned by GET /api/Clubs/{id}/admins. */
export interface ClubAdminUser {
  userId: string;
  personId: string;
  firstName: string;
  lastName: string;
  email: string;
}
