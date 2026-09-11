export type FooterSection = 'Contact' | 'SeasonalSports' | 'OtherActivities';

export const FOOTER_SECTIONS: FooterSection[] = ['Contact', 'SeasonalSports', 'OtherActivities'];

export interface FooterContact {
  id: string;
  title: string;
  details: string | null;
  email: string | null;
  phone: string | null;
  url: string | null;
  sortOrder: number;
  section: FooterSection;
  lastModifiedBy: string | null;
  updatedAt: string;
}

export interface FooterContactRequest {
  title: string;
  details: string | null;
  email: string | null;
  phone: string | null;
  url: string | null;
  sortOrder: number;
  section: FooterSection;
}
