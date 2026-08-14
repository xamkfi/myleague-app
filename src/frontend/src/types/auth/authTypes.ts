export interface AuthTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface AuthUserPerson {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: number;
}

export type AuthUserRole = 'ClubAdmin' | 'SystemAdmin' | 'TeamLeader';

export interface AuthUser {
  id: string;
  email: string;
  personId: string;
  role: AuthUserRole;
  isActive: boolean;
  lastLoginAt: string | null;
  person: AuthUserPerson;
}
