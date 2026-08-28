export type UserRole = 'ClubAdmin' | 'SystemAdmin';

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
  /** Clubs the invited club admin should manage. Only used when role is ClubAdmin. */
  clubAssignments?: string[];
}

export interface UpdateUserPayload {
  email: string;
  role: UserRole;
  isActive: boolean;
}
