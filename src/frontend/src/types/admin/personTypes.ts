export interface Address {
  street1: string;
  street2: string;
  city: string;
  postalCode: string;
  country: string;
}

export interface ContactInfo {
  email: string;
  phone: string;
  alternativePhone: string;
}

export interface Person {
  id: string;
  firstName: string;
  lastName: string;
  birthDate: string;
  fullName: string;
  isRegistered: boolean;
  address?: Address;
  contactInfo?: ContactInfo;
}

export interface PersonFormData {
  firstName: string;
  lastName: string;
  birthDate: string;
  address: Address;
  contactInfo: ContactInfo;
} 