export interface Address {
  street1: string;
  street2: string | null;
  city: string;
  postalCode: string;
  // Optional now — backend treats Country as optional so tournament imports can run without
  // synthesizing a fake country.
  country?: string | null;
}

export interface ContactInfo {
  // Email is optional — backend stops requiring it so tournament imports can create persons
  // without inventing fake addresses.
  email?: string | null;
  phone: string;
  alternativePhone: string | null;
}

/** Fully-populated address block used by admin PersonForm local state (every field present). */
export interface PersonFormAddress {
  street1: string;
  street2: string | null;
  city: string;
  postalCode: string;
  country: string;
}

/** Fully-populated contact block used by admin PersonForm local state (every field present). */
export interface PersonFormContactInfo {
  email: string;
  phone: string;
  alternativePhone: string | null;
}

export const EMPTY_PERSON_FORM_ADDRESS: PersonFormAddress = {
  street1: '',
  street2: null,
  city: '',
  postalCode: '',
  country: '',
};

export const EMPTY_PERSON_FORM_CONTACT_INFO: PersonFormContactInfo = {
  email: '',
  phone: '',
  alternativePhone: null,
};

/** Normalises a partial API address into the shape the admin PersonForm expects. */
export function toPersonFormAddress(partial?: Partial<Address> | null): PersonFormAddress {
  return {
    street1: partial?.street1 ?? '',
    street2: partial?.street2 ?? null,
    city: partial?.city ?? '',
    postalCode: partial?.postalCode ?? '',
    country: partial?.country ?? '',
  };
}

/** Normalises a partial API contact block into the shape the admin PersonForm expects. */
export function toPersonFormContactInfo(partial?: Partial<ContactInfo> | null): PersonFormContactInfo {
  return {
    email: partial?.email ?? '',
    phone: partial?.phone ?? '',
    alternativePhone: partial?.alternativePhone ?? null,
  };
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
  birthDate: string | null;
  fullName: string;
  isRegistered: boolean;
  role: PersonRole;
  address?: Address;
  contactInfo?: ContactInfo;
}

export interface PersonFormData {
  firstName: string;
  lastName: string;
  birthDate: string | null;
  isRegistered: boolean;
  role: PersonRole;
  // Both optional now: tournament imports and other bulk flows often have no address or contact
  // info at all. Sending an empty block used to fail backend validation ("Country is required",
  // "Email is required") so callers should simply leave them out when nothing's known.
  address?: Address;
  contactInfo?: ContactInfo;
}

// Enhanced interface for person creation with optional team assignment.
// Address/contactInfo are required here because the admin form always renders those sections;
// PersonFormData keeps them optional for API payloads (e.g. tournament import).
export interface EnhancedPersonFormData extends Omit<PersonFormData, 'address' | 'contactInfo'> {
  address: PersonFormAddress;
  contactInfo: PersonFormContactInfo;
  teamId?: string;
  position?: string; // Will use FloorballPosition enum values
  jerseyNumber?: number;
}

export interface PaginatedApiResponse<T> {
  success: boolean;
  data: T[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    startItem: number;
    endItem: number;
  };
  message: string;
  errors: string[];
} 