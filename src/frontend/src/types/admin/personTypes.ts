export interface Address {
  street1: string;
  street2: string | null;
  city: string;
  postalCode: string;
  country: string;
}

export interface ContactInfo {
  email: string;
  phone: string;
  alternativePhone: string | null;
}

export enum PersonRole {
  User = 'User',
  Admin = 'Admin',
  SuperAdmin = 'SuperAdmin'
}

export interface Person {
  id: string;
  firstName: string;
  lastName: string;
  birthDate: string;
  fullName: string;
  isRegistered: boolean;
  role: PersonRole;
  address?: Address;
  contactInfo?: ContactInfo;
}

export interface PersonFormData {
  firstName: string;
  lastName: string;
  birthDate: string;
  isRegistered: boolean;
  role: PersonRole;
  address: Address;
  contactInfo: ContactInfo;
}

// Enhanced interface for person creation with optional team assignment
export interface EnhancedPersonFormData extends PersonFormData {
  teamId?: string;
  position?: string; // Will use FloorballPosition enum values
  jerseyNumber?: number;
} 