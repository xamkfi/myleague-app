export type UserRole = 'ClubAdmin' | 'SystemAdmin' | 'TeamLeader';

export interface TeamAssignment {
  sport: 'floorball' | 'football';
  teamId: string;
}

export interface SystemUserPerson {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: number;
}

export interface SystemUser {
  id: string;
  email: string;
  personId: string;
  role: UserRole;
  isActive: boolean;
  isEmailVerified: boolean;
  lastLoginAt: string | null;
  person: SystemUserPerson;
}

export interface CreateUserPayload {
  email: string;
  personId: string;
  role: UserRole;
  /** Teams the invited team leader should manage. Only used when role is TeamLeader. */
  teamAssignments?: TeamAssignment[];
}

export interface UpdateUserPayload {
  email: string;
  role: UserRole;
  isActive: boolean;
}
